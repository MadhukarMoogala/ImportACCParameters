# ImportACCParameters

Importing ACC Parameter definitions created on ACC to AutoCAD, once the definitions are imported in to AutoCAD drawing database, definitions are populated with unique ids against the Selected objects.

This sample demonstrates attaching Asset id to `BlockReferences` and Wire Tags to `Lines` , random values are generated in AutoCAD plugin to attach ids, ideally these uniques values may be stored in SQL Database and retrieved.

## Demo
![](https://github.com/MadhukarMoogala/ImportACCParameters/blob/master/parameters_autocad.png)

## Prequisites

In order to be able to access your BIM 360 project from this demo, you will need to add the following APS credentials as a [custom integration](https://forge.autodesk.com/en/docs/bim360/v1/tutorials/getting-started/manage-access-to-docs):

- APS Client ID:  gXl0SU68l2YK3NXSbdlI1CcjZTPBhNUN

## Build

```bash
git clone https://github.com/MadhukarMoogala/ImportACCParameters.git
cd ImportACCParameters

```

Add a `appsettings.user.json` to the project.

```bash
{
  "Forge": {
    "ClientId": "gXl0SU68l2YK3NXSbdlI1CcjZTPBhNUN",
    "ClientSecret": ""
  }
}
```

- Open the `ImportACCParameters.sln` in Visual Studio 2022

- Rebuild

### License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

### Written by

Madhukar Moogala, [APS Developer Advocacy](https://aps.autodesk.com)  @galakar
