# DatingApp.API
1. To use DBContext, 
Microsoft.EntityFrameworkCore must be installed.

2. To use UseSqlServer method in Startup class, 
Microsoft.EntityFrameworkCore.SqlServer need to be installed 
and make sure you add "using Microsoft.EntityFrameworkCore" in Startup class;

3. To use EntityFramework migration, 
Microsoft.EntityFrameworkCore.Tools needs to be installed. Enable-Migration is obsolete in .NET Core.

4. To use JWT tokens,
Microsoft.IdentityModel.Tokens and System.IdentityModel.Tokens.Jwt need to be installed.
