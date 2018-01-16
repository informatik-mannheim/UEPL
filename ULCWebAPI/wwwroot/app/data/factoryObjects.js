app.factory("Lecture", ($resource, Config) => 
{
	return $resource(Config.API + "lecture/:id", null, { "update": { method: "PUT" }});
});

app.factory("Package", ($resource, Config) => 
{
	return $resource(Config.API + "package/:id", null, { "update": { method: "PUT" }});
});

app.factory("Artifact", ($resource, Config) => 
{
	return $resource(Config.API + "artifact/:id", null, { "update": { method: "PUT" }});
});

app.service("UserService", function() 
{
	let userInfo = {};
	let tokenData = "";
	let validUntil = "";

	userInfo = JSON.parse(localStorage.getItem("User"));
	tokenData = localStorage.getItem("Token");
	validUntil = localStorage.getItem("ValidUntil");

	this.User = (value) => 
	{
		if(!value)
			return angular.copy(userInfo);
		else
		{
			userInfo = value;
			localStorage.setItem("User", JSON.stringify(value));
		}	
	};

	this.Token = (value) => 
	{
		if(!value)
			return angular.copy(tokenData);
		else
		{
			tokenData = value;
			localStorage.setItem("Token", value);
		}
	};

	this.Valid = (value) => 
	{
		if(!value)
			return angular.copy(validUntil);
		else
		{
			validUntil = value;
			localStorage.setItem("ValidUntil", value);
		}
    };

    this.Clear = () => 
    {
        userInfo = {};
        tokenData = "";
        validUntil = "";

        localStorage.removeItem("Token");
        localStorage.removeItem("User");
        localStorage.removeItem("ValidUntil");
    };
});