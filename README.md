ValidateACSTokenWebAPI
======================

Demonstrate how to validate an ACS token in an ASP.NET Web API service

Special thanks to [@woloski](http://twitter.com/#!/woloski) for quickly (and ably) putting
together the [SimpleWebToken](https://github.com/auth10/SimpleWebToken) library for
parsing and validating simple web tokens.

## Usage

Two steps for updating:

1) Update [TokenValidationHandler.cs](https://github.com/wadewegner/ValidateACSTokenWebAPI/blob/master/WebAPI/TokenValidationHandler.cs)
and replace "yourtokensigningkey" with your key from the Access Control Service:

```cs
var validator = new SimpleWebTokenValidator 
{
	SharedKeyBase64 = "yourtokensigningkey"	
}
```

2) Update [AccessControlResources.xaml](https://github.com/wadewegner/ValidateACSTokenWebAPI/blob/master/PhoneApp/Resources/AccessControlResources.xaml)
and replace "youracsnamespace" and "yourrealmname" with the values you specified in
the Access Control Service:

```cs
    <system:String x:Key="acsNamespace">youracsnamespace</system:String>
    <system:String x:Key="realm">uri:yourrealmname</system:String>
```

Enjoy!
