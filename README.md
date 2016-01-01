# sharp2Js
[![Build status](https://ci.appveyor.com/api/projects/status/mrfcabdddrtbrs8v/branch/master?svg=true)](https://ci.appveyor.com/project/tghamm/sharp2js/branch/master) [![Coverage Status](https://coveralls.io/repos/castle-it/sharp2Js/badge.svg?branch=master&service=github)](https://coveralls.io/github/castle-it/sharp2Js?branch=master) [![GitHub version](https://badge.fury.io/gh/castle-it%2Fsharp2js.svg)](https://badge.fury.io/gh/castle-it%2Fsharp2js) [![NuGet version](https://badge.fury.io/nu/sharp2js.svg)](https://badge.fury.io/nu/sharp2js)

`sharp2Js` is a small library that can create javascript objects that mirror your `C#` POCO classes and can be easily used to generate js files using `T4` templates.

Features (v1.3.0)
--
* Optionally generates merge map function to make applying model changes easy
* Handle custom types, primitives, arrays, recursive types, structs, enums, and collections (`IList`, `ICollection`, `IDictionary`)
* Allows for an override constructor if you need to wrap the created objects (e.g. for Angular)
* Outputs to string for easy addition to `T4` template output or otherwise
* Supports optional camel casing
* Supports optional automatic removal of phrases like Dto, etc
* Supports the `[DefaultValue]` attribute for primitive types
* Supports `[IgnoreDataMember]` and `[DataMember]` attributes
* Supports custom function injection into generated Js objects

Examples/How-to
--
* [Getting Started](https://github.com/castle-it/sharp2Js/wiki/Getting-Started)
* [Integrating with AngularJs](https://github.com/castle-it/sharp2Js/wiki/Integrating-with-AngularJs)

Installation
---
`sharp2Js` can be installed via the nuget UI (as sharp2Js), or via the nuget package manager console:
```
PM> Install-Package sharp2Js
```
Sample T4 Example
---
```C#
<#@ template debug="false" hostspecific="false" language="C#" #>
<#@ assembly name="System.Core" #>
<#@ import namespace="System.Linq" #>
<#@ import namespace="System.Text" #>
<#@ import namespace="System.Collections.Generic" #>
<#@ output extension=".js" #>
<#@ assembly name="$(ProjectDir)bin\$(ConfigurationName)\Castle.Sharp2Js.dll" #>
<#@ output extension=".js" #>
<# var str = Castle.Sharp2Js.JsGenerator.Generate(new [] { typeof(Castle.Sharp2Js.SampleData.AddressInformation) }); #>
models = {};

<#=str#>
```
C# POCOs Example
---
```C#
public class AddressInformation
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int ZipCode { get; set; }
        public OwnerInformation Owner { get; set; }
        public List<Feature> Features { get; set; }
        public List<string> Tags { get; set; }
    }

    public class OwnerInformation
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    public class Feature
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }
```
Javascript Object Output
---
```JavaSscript
models = {};

models.AddressInformation = function (cons, overrideObj) {
	if (!overrideObj) { overrideObj = { }; }
	if (!cons) { cons = { }; }
	var i, length;

	this.Name = cons.Name;
	this.Address = cons.Address;
	this.ZipCode = cons.ZipCode;
	if (!overrideObj.OwnerInformation) {
		this.Owner = new models.OwnerInformation(cons.Owner);
	} else {
		this.Owner = new overrideObj.OwnerInformation(cons.Owner, overrideObj);
	}
	this.Features = new Array(cons.Features == null ? 0 : cons.Features.length );
	if(cons.Features != null) {
		for (i = 0, length = cons.Features.length; i < length; i++) {
			if (!overrideObj.Feature) {
				this.Features[i] = new models.Feature(cons.Features[i]);
			} else {
				this.Features[i] = new overrideObj.Feature(cons.Features[i], overrideObj);
			}
		}
	}
	this.Tags = new Array(cons.Tags == null ? 0 : cons.Tags.length );
	if(cons.Tags != null) {
		for (i = 0, length = cons.Tags.length; i < length; i++) {
			this.Tags[i] = cons.Tags[i];
		}
	}
}


models.OwnerInformation = function (cons) {
	if (!cons) { cons = { }; }

	this.FirstName = cons.FirstName;
	this.LastName = cons.LastName;
	this.Age = cons.Age;
}


models.Feature = function (cons) {
	if (!cons) { cons = { }; }

	this.Name = cons.Name;
	this.Value = cons.Value;
}

```

Change Log
---
#### v1.4.0 
###### January 1, 2016
* Adds support for an `$equals` method that can be used to perform deep comparisons on objects
* Fixes an issue that caused `bool` `DefaultValue` values to be rendered improperly
* Switches to Appveyor CI
* Adds coveralls/OpenCover support

#### v1.3.0
###### November 27, 2015
* Adds support for `Enum` types
* Adds support for custom function parsers if you want to add your own custom functions to the output
* Adds support for all `IList`, `ICollection`, and `IDictionary` types
* Light internal refactoring for maintainability

#### v1.2.0
###### November 16, 2015
* Fixes the camel-casing implementation
* Fixes an issue with improperly initialized object types
* Adds support for the `DefaultValueAttribute` on primitive types
* Test coverage upped to 100%

#### v1.1.1
###### November 5, 2015
* Adds Slack integration to CI
* Switches to NUnit for unit tests
* Adds support for `DataMember` and `IgnoreDataMember` attributes

#### v1.1.0
###### October 24, 2015
* Adds support for array and recursive types
* Includes NuGet package
* Adds `$merge` function to perform a deep merge on objects

#### v1.0.0
###### October 20, 2015
* Initial Commit/Version

Contributions
---
Any improvements are welcome.  `sharp2Js` has served our limited purposes so far, but we would love to see it grow.
