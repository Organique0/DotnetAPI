https://www.udemy.com/course/net-core-with-ms-sql-beginner-to-expert

az login

dotnet build --configuration Release

az webapp up --sku F1 --name "LGdontnetAPI" --os-type linux

az webapp delete --name "AppName"

az group delete --name "GroupName"

az sql db delete --name "DatabaseName"

az sql server delete --name "server-name"


Database is under worse reddit group
When you deploy, you need to copy content of appsettings.Development.json to appsettings.json 
so that Azure can auto get all of the values while not exposing it to Github


All SQL scripts are in the SQLCheatsheet.sql. 
To seed the DB there is another project custom made for it here: https://github.com/DomTripodi93/DotNetAPICourse


This is an API only. There is no front end.
To log in, you need to register, log in with your credentials to get a token, then use Postman with Authorization header set to "Bearer" + that token value in order to access authorized endpoints.
You can also use Swagger on localhost:5000/swagger/index.html to test the api.
