# sharp2Js
[![Build Status](https://travis-ci.org/castle-it/sharp2Js.svg?branch=master)](https://travis-ci.org/castle-it/sharp2Js)

`sharp2Js` is a small library that can create javascript objects that mirror your `C#` POCO classes and can be easily used to generate js files using `T4` templates.

Features (v1.2.0)
--
* Optionally generates merge map function to make applying model changes easy
* Handle custom types, primitives, arrays, recursive types, structs, and `List<T>`
* Allows for an override constructor if you need to wrap the created objects (e.g. for Angular)
* Outputs to string for easy addition to `T4` template output or otherwise
* Supports optional camel casing
* Supports optional automatic removal of phrases like Dto, etc.
* Supports the `[DefaultValue]` attribute for primitive types
* Supports `[IgnoreDataMember]` and `[DataMember]` attributes
* Test Coverage at 100%

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

	this.$merge = function (mergeObj) {
		if (!mergeObj) { mergeObj = { }; }
		this.Name = mergeObj.Name;
		this.Address = mergeObj.Address;
		this.ZipCode = mergeObj.ZipCode;
		if (mergeObj.Owner == null) {
			this.Owner = null;
		} else if (this.Owner != null) {
			this.Owner.$merge(mergeObj.Owner);
		} else {
			this.Owner = mergeObj.Owner;
		}
		if (!mergeObj.Features) {
			this.Features = null;
		}
		if (this.Features != null) {
			this.Features.splice(0, this.Features.length);
		}
		if (mergeObj.Features) {
			if (this.Features === null) {
				this.Features = [];
			}
			for (i = 0; i < mergeObj.Features.length; i++) {
				this.Features.push(mergeObj.Features[i]);
			}
		}
		if (!mergeObj.Tags) {
			this.Tags = null;
		}
		if (this.Tags != null) {
			this.Tags.splice(0, this.Tags.length);
		}
		if (mergeObj.Tags) {
			if (this.Tags === null) {
				this.Tags = [];
			}
			for (i = 0; i < mergeObj.Tags.length; i++) {
				this.Tags.push(mergeObj.Tags[i]);
			}
		}
	}
}


models.OwnerInformation = function (cons) {
	if (!cons) { cons = { }; }

	this.FirstName = cons.FirstName;
	this.LastName = cons.LastName;
	this.Age = cons.Age;

	this.$merge = function (mergeObj) {
		if (!mergeObj) { mergeObj = { }; }
		this.FirstName = mergeObj.FirstName;
		this.LastName = mergeObj.LastName;
		this.Age = mergeObj.Age;
	}
}


models.Feature = function (cons) {
	if (!cons) { cons = { }; }

	this.Name = cons.Name;
	this.Value = cons.Value;

	this.$merge = function (mergeObj) {
		if (!mergeObj) { mergeObj = { }; }
		this.Name = mergeObj.Name;
		this.Value = mergeObj.Value;
	}
}

```
AngularJs
--
Our use cases revolved around using AngularJs and using models to manage the data in our services and directives allowing for easy newing up of models and having calculated fields of methods to use with the model.

We allow for simple wrapping of the objects if you need dependency injection as well as allowing for overriding object definitions down the tree of complex objects.

Simple Angular Dependency Wrapping
```JavaScript
(function () {
    angular.module('models.AddressInformation', []).factory('AddressInformation', AddressInformationModel);

    AddressInformationModel.$inject = [];

    function AddressInformationModel() {
        /**
         * @class AddressInformation
         * @description AddressInformation is a an object defined in C# code
         * @constructor
         */
        function AddressInformation(addressInformation) {
            if(!addressInformation)
                throw 'Cannot create a new AddressInformation: Please pass existing AddressInformation object or empty object to create new AddressInformation';
            models.AddressInformation.apply(this, [addressInformation]);
        }

        AddressInformation.prototype = models.AddressInformation.prototype;
        AddressInformation.prototype.constructor = AddressInformation;
        return AddressInformation;
    }
}());

(function(){
    angular.module('myApp.services', ['models.AddressInformation']).factory('addressService', addressService);
    addressService.$inject = ['AddressInformation'];
    function addressService(AddressInformation) {
    	var service = {
    		getDefaultAddressInformation: getDefaultAddressInformation
    	};
    	return service;
    	
    	function getDefaultAddressInformation() {
    		return new AddressInformation({
    			tags: ['New Address'],
    			zipCode: 27513
    		});
    	}
    }
}());
```
Extending Model Functionality, we use underscode '_' for any properties or methods generated in javascript code, not for any particular reason, just for pattern consistency and readability. Any '_myProperty' you then know is maintained by JS extentions.
```JavaScript
(function () {
    angular.module('models.AddressInformation', []).factory('AddressInformation', AddressInformationModel);

    AddressInformationModel.$inject = ['$filter'];

    function AddressInformationModel($filter) {
        /**
         * @class AddressInformation
         * @description AddressInformation is a an object defined in C# code
         * @constructor
         */
        function AddressInformation(addressInformation) {
            if(!addressInformation)
                throw 'Cannot create a new AddressInformation: Please pass existing AddressInformation object or empty object to create new AddressInformation';
            models.AddressInformation.apply(this, [addressInformation]);
            
            //Calculated fields 
            this.__defineGetter__('_googleMapsCityName', function () {
                return $filter('zipcodeToGoogleMapsCityName')(this.zipCode);
            });
            
            this._myVar = 'This is JS extensions';
        }

        AddressInformation.prototype = models.AddressInformation.prototype;
        AddressInformation.prototype.constructor = AddressInformation;
        AddressInformation.prototype.getValidationError = getValidationError;
        return AddressInformation;
        
        //validation requires atleast a name
        function getValidationError() {
        	if(!this.name)
        	    return 'Address Information requires a name.';
        	return null;
        }
    }
}());

(function(){
    angular.module('myApp.services', ['models.AddressInformation']).factory('addressService', addressService);
    addressService.$inject = ['AddressInformation', '$q', '$http'];
    function addressService(AddressInformation, $q, $http) {
    	var service = {
    		getDefaultAddressInformation: getDefaultAddressInformation,
    		saveAddressInformation: saveAddressInformation
    	};
    	return service;
    	
    	function getDefaultAddressInformation() {
    		return new AddressInformation({
    			tags: ['New Address'],
    			zipCode: 27513
    		});
    	}
    	
    	function saveAddressInformation(addressInformation) {
    		return $q(function(resolve, reject){
    			//cast into the JS object, just in case we are not
    			addressInformation = new AddressInformation(addressInformation);
    			var error = addressInformation.getValidationError();
    			if(error) {
    				reject(error)
    			} else {
    				$http();//.....
    			}
    		});
    	}
    }
}());
```

Sometimes you override child objects, such as Owner Information and need some JS extentions there as well, This can be achieved by using overrides as a second param to the javascript constructor

```JavaScript
(function () {
    angular.module('models.AddressInformation', ['models.OwnerInformation']).factory('AddressInformation', AddressInformationModel);

    AddressInformationModel.$inject = ['$filter', 'OwnerInformation'];

    function AddressInformationModel($filter, OwnerInformation) {
        /**
         * @class AddressInformation
         * @description AddressInformation is a an object defined in C# code
         * @constructor
         */
        function AddressInformation(addressInformation) {
            if(!addressInformation)
                throw 'Cannot create a new AddressInformation: Please pass existing AddressInformation object or empty object to create new AddressInformation';
                
            //use the angular defined object as the model when we create it as a dependency    
            var overrides = {
            	OwnerInformation: OwnerInformation
            }
            models.AddressInformation.apply(this, [addressInformation, overrides]);
            
            //Calculated fields 
            this.__defineGetter__('_googleMapsCityName', function () {
            	//HTML Binding <span>{{addressInformation._googleMapsCityName}}</span>
                return $filter('zipcodeToGoogleMapsCityName')(this.zipCode);
            });
            
            this._myVar = 'This is JS extensions';
        }

        AddressInformation.prototype = models.AddressInformation.prototype;
        AddressInformation.prototype.constructor = AddressInformation;
        AddressInformation.prototype.getValidationError = getValidationError;
        return AddressInformation;
        
        //validation requires atleast a name
        function getValidationError() {
        	if(!this.name)
        	    return 'Address Information requires a name.';
        	return null;
        }
    }
}());

(function () {
    angular.module('models.OwnerInformation', []).factory('OwnerInformation', OwnerInformationModel);

    OwnerInformationModel.$inject = ['$filter'];

    function OwnerInformationModel($filter) {
        /**
         * @class OwnerInformation
         * @description OwnerInformation is a an object defined in C# code
         * @constructor
         */
        function OwnerInformation(ownerInformation) {
            if(!ownerInformation)
                throw 'Cannot create a new OwnerInformation: Please pass existing OwnerInformation object or empty object to create new OwnerInformation';
            models.OwnerInformation.apply(this, [ownerInformation]);
            
            //Calculated fields 
            this.__defineGetter__('_fullName', function () {
                return this.firstName + ' ' + this.lastName;
            });
        }

        OwnerInformation.prototype = models.OwnerInformation.prototype;
        OwnerInformation.prototype.constructor = OwnerInformation;
        OwnerInformation.prototype.getValidationError = getValidationError;
        return OwnerInformation;
        
        //validation requires atleast a name
        function getValidationError() {
        	if(!this.firstName || !this.lastName)
        	    return 'Owner Information requires a first and last name.';
        	return null;
        }
    }
}());

(function(){
    angular.module('myApp.services', ['models.AddressInformation']).factory('addressService', addressService);
    addressService.$inject = ['AddressInformation', '$q', '$http'];
    function addressService(AddressInformation, $q, $http) {
    	var service = {
    		getDefaultAddressInformation: getDefaultAddressInformation,
    		saveAddressInformation: saveAddressInformation
    	};
    	return service;
    	
    	function getDefaultAddressInformation() {
    		return new AddressInformation({
    			tags: ['New Address'],
    			zipCode: 27513
    		});
    	}
    	
    	function saveAddressInformation(addressInformation) {
    		return $q(function(resolve, reject){
    			//cast into the JS object, just in case we are not
    			addressInformation = new AddressInformation(addressInformation);
    			var error = addressInformation.getValidationError();
    			if(!error) {
    				error = addressInformation.ownerInformation.getValidationError();
    			}
    			
    			if(error) {
    				reject(error)
    			} else {
    				$http();//.....
    			}
    		});
    	}
    }
}());
```

Contributions
---
Any improvements are welcome.  `sharp2Js` has served our limited purposes so far, but we would love to see it grow.
