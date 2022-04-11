module Tests

open Expecto
open Oceanbox.GeoJson

let geoJson = """
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "geometry": {
        "type": "Point",
        "coordinates": [102.0, 0.5]
      },
      "properties": {
        "prop0": "value0"
      }
    },
    {
      "type": "Feature",
      "geometry": {
        "type": "LineString",
        "coordinates": [
          [102.0, 0.0], [103.0, 1.0], [104.0, 0.0], [105.0, 1.0]
        ]
      },
      "properties": {
        "prop0": "value0",
        "prop1": 0.0
      }
    },
    {
      "type": "Feature",
      "geometry": {
        "type": "Polygon",
        "coordinates": [
          [
            [100.0, 0.0], [101.0, 0.0], [101.0, 1.0],
            [100.0, 1.0], [100.0, 0.0]
          ]
        ]
      },
      "properties": {
        "prop0": "value0",
        "prop1": { "this": "that" }
      }
    }
  ]
}
"""

let readCollection () =
    let fc = FeatureCollection.Decode geoJson
    match fc with
    | Ok json ->
        printfn $"%A{json.BBox}"
        printfn $"%A{json.Features}"
    | Error e ->
        printfn $"{e}"
    fc

let encodeCollection (json: FeatureCollection) = json.Encode ()

let testCollections =
    testList
        "FeatureCollection"
        [
            testCase "Decode"
            <| fun _ ->
                let fc = readCollection ()
                Expect.isOk fc "Result should be ok"
            testCase "Encode"
            <| fun _ ->
                match readCollection () with
                | Ok fc ->
                    let json = fc.Encode ()
                    Expect.isNotEmpty json "Json is not empty"
                | Error e -> Expect.isEmpty e "Error is empty"
        ]

let all = testList "All" [ testCollections ]

[<EntryPoint>]
let main _ = runTests defaultConfig all