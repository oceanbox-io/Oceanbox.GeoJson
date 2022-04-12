# Oceanbox.GeoJson

## Build

`dotnet run`

## Package

`dotnet run pack`

## Example usage

```fsharp
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
        "type": "Polygon",
        "coordinates": [
          [
            [100.0, 0.0], [101.0, 0.0], [101.0, 1.0],
            [100.0, 1.0], [100.0, 0.0]
          ]
        ]
      }
    }
  ]
}
"""

let readJson () =
    let result = FeatureCollection.Decode geoJson
    match result with
    | Ok json -> printfn $"%A{json.Features}"
    | Error e -> printfn $"decode failed: {e}"

let encodeJson (json: FeatureCollection) = json.Encode ()
```