module Oceanbox.GeoJson

open System
#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif

type Coord =
    | C2 of float * float
    | C3 of float * float * float
with
    member __.get () =
        match __ with
        | C2 (x, y) -> x, y, 0.0
        | C3 (x, y, z) -> x, y, z

    member __.getXY () =
        match __ with
        | C2 (x, y) -> x, y
        | C3 (x, y, _) -> x, y

    member __.getZ () =
        match __ with
        | C2 _ -> 0.0
        | C3 (_, _, z) -> z

    static member apply f =
        function
        | C2 (x, y) -> C2 (f (x, y))
        | C3 (x, y, z) ->
           let x', y' = (f (x, y))
           C3 (x', y', z)

    static member apply (f, g) =
        function
        | C2 (x, y) -> C2 (f x, g y)
        | C3 (x, y, z) -> C3 (f x, g y, z)

type MultiPoint = Coord []
type LineString = Coord []
type Polygon = Coord [] []
type MultiLineString = LineString []
type MultiPolygon = Polygon []

type GeometryObject =
    | Point of Coord
    | MultiPoint of MultiPoint
    | LineString of LineString
    | Polygon of Polygon
    | MultiLineString of MultiLineString
    | MultiPolygon of MultiPolygon
    | GeometryCollection of GeometryObject []
with
    member __.Encoder () =
        let encodePos p =
            match p with
            | C2 (x, y) -> (Encode.array << Array.map Encode.float) [| x; y |]
            | C3 (x, y, z) -> (Encode.array << Array.map Encode.float) [| x; y; z |]
        let encodeCoord = Encode.array << Array.map encodePos
        let encodePoints = Encode.array << Array.map encodeCoord
        let encodeSegments = encodePoints
        let encodePolygons = Encode.array << Array.map encodePoints
        // let encodeSegments = Encode.array << Array.map encodePoints
        match __ with
        | Point x -> Encode.object [
            "type", Encode.string "Point"
            "coordinates", encodeCoord [| x |]
            ]
        | MultiPoint x -> Encode.object [
            "type", Encode.string "MultiPoint"
            "coordinates", encodePoints [| x |]
            ]
        | LineString x -> Encode.object [
            "type", Encode.string "LineString"
            "coordinates", encodePoints [| x |]
            ]
        | Polygon x -> Encode.object [
            "type", Encode.string "Polygon"
            "coordinates", encodeSegments x
            ]
        | MultiLineString x -> Encode.object [
            "type", Encode.string "MultiLineString"
            "coordinates", encodeSegments x
            ]
        | MultiPolygon x -> Encode.object [
            "type", Encode.string "MultiPolygon"
            "coordinates", encodePolygons x
            ]
        | GeometryCollection x -> Encode.object [
            "type", Encode.string "GeometryCollection"
            "geometries", Array.map (fun (g : GeometryObject) ->
                g.Encoder ()) x
                |> Encode.array
            ]

    static member Decoder : Decoder<_> =
        let decodeCoord = Decode.array Decode.float
        let decodePoints = Decode.array decodeCoord
        let decodeSegments = Decode.array decodePoints
        let rec decoder () =
            Decode.object (fun get ->
                let inline getCoords (f :
                                string ->
                                JsonValue ->
                                Result<'T array, DecoderError>) =
                    get.Required.Field "coordinates" f

                let toCoord c =
                    c
                    |> List.ofArray
                    |> function
                        | [x; y] -> C2 (x, y)
                        | [x; y; z] -> C3 (x, y, z)
                        | _ -> failwith "invalid coordinate data"
                let coord () = getCoords decodeCoord |> toCoord
                let points () = getCoords decodePoints |> Array.map toCoord
                let segments () =
                    getCoords decodeSegments
                    |> Array.map (Array.map toCoord)
                match get.Required.Field "type" Decode.string with
                | "Point" -> coord () |> Point
                | "MultiPoint" -> points () |> MultiPoint
                | "LineString" -> points () |> LineString
                | "Polygon" -> segments () |>  Polygon
                | "MultiLineString" -> segments () |> MultiLineString
                | "MultiPolygon" -> segments () |> MultiLineString
                | "GeometryCollection" ->
                    get.Required.Field "geometries" (Decode.array (decoder ()))
                    |> GeometryCollection
                | _ -> failwith "error"
            )
        decoder ()

    member __.Encode () =
        let x = __.Encoder ()
        Fable.Core.JS.console.log x
        __.Encoder () |> Encode.toString 2
    static member Decode x = Decode.fromString GeometryObject.Decoder x

    static member apply f geo =
        let t1 = Array.map f
        let t2 = Array.map t1
        match geo with
        | Point x -> Point (f x)
        | MultiPoint x -> MultiPoint (t1 x)
        | LineString x -> LineString (t1 x)
        | Polygon x -> Polygon (t2 x)
        | MultiLineString x -> MultiLineString (t2 x)
        | MultiPolygon x -> MultiPolygon (Array.map t2 x)
        | GeometryCollection x ->
            GeometryCollection (x |> Array.map (GeometryObject.apply f))

type BBox =
    {
        MinX: float
        MinY: float
        MaxX: float
        MaxY: float
    }
with
    static member initial =
        {
            MinX =  infinity
            MinY =  infinity
            MaxX = -infinity
            MaxY = -infinity
        }
    static member infinite =
        {
            MinX = -infinity
            MinY = -infinity
            MaxX =  infinity
            MaxY =  infinity
        }
    static member create (box: float array) =
        {
            MinX = box.[0]
            MinY = box.[1]
            MaxX = box.[2]
            MaxY = box.[3]
        }
    static member amend box1 box2 =
        {
            MinX = Math.Min (box1.MinX, box2.MinX)
            MinY = Math.Min (box1.MinY, box2.MinY)
            MaxX = Math.Max (box1.MaxX, box2.MaxX)
            MaxY = Math.Max (box1.MaxY, box2.MaxY)
        }
    static member toArray box =
        [|
            box.MinX
            box.MinY
            box.MaxX
            box.MaxY
        |]

type Feature<'T> =
    {
        Geometry : GeometryObject
        Properties : 'T option
        BBox : BBox option
        Id : int option
        Size: float []
    }
with
    member __.Encoder (?propEncoder : 'T -> JsonValue) =
        let props =
            match __.Properties, propEncoder with
            | Some props, Some encode ->  [ "properties", encode props ]
            | _ -> []
        let bbox =
            match __.BBox with
            | Some b -> [ "bbox",  Array.map Encode.float (BBox.toArray b) |> Encode.array ]
            | _ -> []
        let id =
            match __.Id with
            | Some id -> [ "id", Encode.int id ]
            | _ -> []
        let feature = [
                "type", Encode.string "Feature"
                "geometry", __.Geometry.Encoder ()
            ]
        let all = List.concat [ feature; bbox; id; props ]
        Encode.object all

    static member Decoder (?propDecoder : Decoder<'T>) =
        Decode.object (fun get ->
            {
                Geometry = get.Required.Field "geometry" GeometryObject.Decoder
                BBox = get.Optional.Field "bbox" (Decode.array Decode.float) |> Option.map BBox.create
                Id = get.Optional.Field "id" Decode.int

                Properties =
                    if propDecoder.IsSome then
                        get.Optional.Field "properties" propDecoder.Value
                    else
                        None
                Size = Array.empty
            }
        )

    member __.Encode (?propEncoder : 'T -> JsonValue) =
        match propEncoder with
        | Some e -> __.Encoder e
        | None -> __.Encoder ()
        |> Encode.toString 2

    static member Decode (x, ?propDecoder : Decoder<'T>) =
        let d =
            match propDecoder with
            | Some x -> Feature<'T>.Decoder x
            | None -> Feature<'T>.Decoder ()
        Decode.fromString d x

    static member apply f x =
        {
            x with Geometry = GeometryObject.apply f x.Geometry
        }

type FeatureCollection<'T> =
    {
        Features : Feature<'T> []
        BBox : BBox option
    }
with
    member __.Encoder (?propEncoder : 'T -> JsonValue) =
        let fencoder (f : Feature<'T>) =
            match propEncoder with
            | Some x -> f.Encoder x
            | None   -> f.Encoder ()
        Encode.object [
            "type", Encode.string "FeatureCollection"
            "bbox", Encode.option (fun (a : BBox) ->
                Array.map Encode.float (BBox.toArray a)
                |> Encode.array
                ) __.BBox
            "features", Encode.array (Array.map fencoder __.Features)
        ]

    static member Decoder (?propDecoder : Decoder<'T>) : Decoder<FeatureCollection<'T>> =
        let fdecoder =
            match propDecoder with
            | Some x -> Feature<'T>.Decoder x
            | None   -> Feature<'T>.Decoder ()
        Decode.object (fun get ->
            {
                Features = get.Required.Field "features" (Decode.array fdecoder)
                BBox =
                    get.Optional.Field "bbox" (Decode.array Decode.float)
                    |> Option.map BBox.create
            }
        )

    member __.Encode (?propEncoder : 'T -> JsonValue) =
        match propEncoder with
        | Some e -> __.Encoder e
        | None   -> __.Encoder ()
        |> Encode.toString 2

    static member Decode (x, ?propDecoder : Decoder<'T>) =
        let d =
            match propDecoder with
            | Some x -> FeatureCollection<'T>.Decoder x
            | None   -> FeatureCollection<'T>.Decoder ()
        Decode.fromString d x

    static member apply f = Array.map (Feature<_>.apply f)

type Feature = Feature<unit>

type FeatureCollection = FeatureCollection<unit>
