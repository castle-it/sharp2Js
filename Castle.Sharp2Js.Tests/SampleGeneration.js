castle = {};

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



