+++++++++++++++++++++++++++++++++++++++++++
SOCIALAUTH.NET v2.3                       |
http://code.google.com/p/socialauth-net/  |
+++++++++++++++++++++++++++++++++++++++++++

Contents
========

1. WebformsDemo: Contains demo WebForms application illustrating many of SocialAuth.net features


2. MVC3Demo: Simple demo to use SocialAuth.NET in MVC along with default authentication. Help on using this Demo is available at http://code.google.com/p/socialauth-net/wiki/Using_MVC_Demo

3. NuGe tPackage: Nuget package which adds all required DLLs and makes necessary web.config updates. This is same NuGet package as what is available online (search "Socialauth.net"). NuGet package does following things:
a. Adds DLLs in Lib folder to your project
b. Updates WebConfig with SocialAuth.NET sections
Help in using Nuget package is available at http://code.google.com/p/socialauth-net/wiki/tutorial_NuGet_package_installation

4. Lib: Folder containing DLLs required for SocialAuth.NET 

5. SourceCode: Core library sourcecode




New Features/Improvements in SocialAuth.NET 2.3
==============================================
1. Now compatible with  sql and stateserver session stores
2. Login argument in Login() will now work in windows FormsAuthentication mode as well
3. NuGet package added
4. Bugs resolved
5. If you login using your custom/forms authentication mechanism, SocialAuth.NET wouldn't override Identity now

