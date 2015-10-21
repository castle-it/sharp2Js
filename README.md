# sharp2Js
- - -
[![Build Status](https://travis-ci.org/castle-it/sharp2Js.svg?branch=master)](https://travis-ci.org/castle-it/sharp2Js)

`sharp2Js` is a small library that can create javascript objects that mirror your `C#` POCO classes, and you can easily redirect that output to `T4` templates.

### Features
* Generates merge map function to make applying model changes easy
* Handle custom types, primitives, and `List<T>`
* Allows for an override constructor if you need to wrap the created objects
* Outputs to string for easy addition to `T4` template output
* Supports camel casing

### Sample T4 Example
```C#
<#@ template debug="false" hostspecific="false" language="C#" #>
<#@ assembly name="System.Core" #>
<#@ import namespace="System.Linq" #>
<#@ import namespace="System.Text" #>
<#@ import namespace="System.Collections.Generic" #>
<#@ output extension=".js" #>
<#@ assembly name="$(SolutionDir)Castle.Sharp2Js.Tests\bin\Debug\Castle.Sharp2Js.dll" #>
<#@ assembly name="$(SolutionDir)Castle.Sharp2Js.Tests\bin\Debug\Castle.Sharp2Js.Tests.dll" #>
<#@ output extension=".js" #>
<# var str = Castle.Sharp2Js.JsGenerator.GenerateJsModelFromTypeWithDescendants(typeof(Castle.Sharp2Js.Tests.DTOs.AddressInformation), true, "castle"); #>
castle = {};

<#=str#>
```


### C# POCOs
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

### Javascript Object Output
```JavaSscript
castle.AddressInformation = function (cons, overrideObj) {
	if (!overrideObj) { overrideObj = { }; }
	if (!cons) { cons = { }; }
	var i, length;
	this.name = cons.name;
	this.address = cons.address;
	this.zipCode = cons.zipCode;
	if (!overrideObj.OwnerInformation) {
		this.owner = new castle.OwnerInformation(cons.owner);
	} else {
		this.owner = new overrideObj.OwnerInformation(cons.owner, overrideObj);
	}
	this.features = new Array(cons.features == null ? 0 : cons.features.length );
	if(cons.features != null) {
		for (i = 0, length = cons.features.length; i < length; i++) {
			if (!overrideObj.Feature) {
				this.features[i] = new castle.Feature(cons.features[i]);
			} else {
				this.features[i] = new overrideObj.Feature(cons.features[i], overrideObj);
			}
		}
	}
	this.tags = new Array(cons.tags == null ? 0 : cons.tags.length );
	if(cons.tags != null) {
		for (i = 0, length = cons.tags.length; i < length; i++) {
			this.tags[i] = cons.tags[i];
		}
	}


	this.$merge = function (mergeObj) {
		if (!mergeObj) { mergeObj = { }; }
		this.name = mergeObj.name;
		this.address = mergeObj.address;
		this.zipCode = mergeObj.zipCode;
		if (mergeObj.owner == null) {
			this.owner = null;
		} else if (this.owner != null) {
			this.owner.$merge(mergeObj.owner);
		} else {
			this.owner = mergeObj.owner;
		}
		if (!mergeObj.features) {
			this.features = null;
		}
		if (this.features != null) {
			this.features.splice(0, this.features.length);
		}
		if (mergeObj.features) {
			if (this.features === null) {
				this.features = [];
			}
			for (i = 0; i < mergeObj.features.length; i++) {
				this.features.push(mergeObj.features[i]);
			}
		}
		if (!mergeObj.tags) {
			this.tags = null;
		}
		if (this.tags != null) {
			this.tags.splice(0, this.tags.length);
		}
		if (mergeObj.tags) {
			if (this.tags === null) {
				this.tags = [];
			}
			for (i = 0; i < mergeObj.tags.length; i++) {
				this.tags.push(mergeObj.tags[i]);
			}
		}
	}
}


castle.OwnerInformation = function (cons, overrideObj) {
	if (!overrideObj) { overrideObj = { }; }
	if (!cons) { cons = { }; }
	var i, length;
	this.firstName = cons.firstName;
	this.lastName = cons.lastName;
	this.age = cons.age;


	this.$merge = function (mergeObj) {
		if (!mergeObj) { mergeObj = { }; }
		this.firstName = mergeObj.firstName;
		this.lastName = mergeObj.lastName;
		this.age = mergeObj.age;
	}
}


castle.Feature = function (cons, overrideObj) {
	if (!overrideObj) { overrideObj = { }; }
	if (!cons) { cons = { }; }
	var i, length;
	this.name = cons.name;
	this.value = cons.value;


	this.$merge = function (mergeObj) {
		if (!mergeObj) { mergeObj = { }; }
		this.name = mergeObj.name;
		this.value = mergeObj.value;
	}
}
```

### Contributions
Any improvements are welcome.  `sharp2Js` has served our limited purposes so far, but we would love to see it grow.
