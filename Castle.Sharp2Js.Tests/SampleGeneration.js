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



