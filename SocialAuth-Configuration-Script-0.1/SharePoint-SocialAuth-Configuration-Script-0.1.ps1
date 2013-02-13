cd\
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2("C:\Certificates\sts.brickred.com.cer")
$map = New-SPClaimTypeMapping "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn" -IncomingClaimTypeDisplayName "UPN" -SameAsIncoming
$realm = "http://sharepoint2010.brickred.com/_trust/"
$signinurl = "http://sts.brickred.com:4321/STS/Default.aspx"

New-SPTrustedRootAuthority -Name "BrickRed STS" -Certificate $cert

$ap = New-SPTrustedIdentityTokenIssuer -Name "Social Auth" -Description "BrickRed SocailAuth Security Token Service" -Realm $realm -ImportTrustCertificate $cert -ClaimsMappings $map -SignInUrl $signinurl -IdentifierClaim $map.InputClaimType 
$ap.UseWReplyParameter = 1;
$ap.update();


$ap =Get-SPTrustedIdentityTokenIssuer
$ap.ClaimTypes.add("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
$ap.ClaimTypes.add("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication");
$ap.ClaimTypes.add("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/displayname");
$ap.ClaimTypes.add("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth");
$ap.ClaimTypes.add("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri");
$ap.ClaimTypes.add("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
$ap.ClaimTypes.add("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
$ap.ClaimTypes.add("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender");

$ap.update();
$ap =Get-SPTrustedIdentityTokenIssuer

$map = New-SPClaimTypeMapping "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" -IncomingClaimTypeDisplayName "Role" -SameAsIncoming
Add-SPClaimTypeMapping -Identity $map -TrustedIdentityTokenIssuer $ap
$ap.update();
$ap =Get-SPTrustedIdentityTokenIssuer

$map = New-SPClaimTypeMapping "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication" -IncomingClaimTypeDisplayName "Authentication" -SameAsIncoming
Add-SPClaimTypeMapping -Identity $map -TrustedIdentityTokenIssuer $ap
$ap.update();
$ap =Get-SPTrustedIdentityTokenIssuer

$map = New-SPClaimTypeMapping "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/displayname" -IncomingClaimTypeDisplayName "Display Name" -SameAsIncoming
Add-SPClaimTypeMapping -Identity $map -TrustedIdentityTokenIssuer $ap
$ap.update();
$ap =Get-SPTrustedIdentityTokenIssuer

$map = New-SPClaimTypeMapping "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth" -IncomingClaimTypeDisplayName "DOB" -SameAsIncoming
Add-SPClaimTypeMapping -Identity $map -TrustedIdentityTokenIssuer $ap
$ap.update();
$ap =Get-SPTrustedIdentityTokenIssuer

$map = New-SPClaimTypeMapping "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri" -IncomingClaimTypeDisplayName "PictureURL" -SameAsIncoming
Add-SPClaimTypeMapping -Identity $map -TrustedIdentityTokenIssuer $ap
$ap.update();
$ap =Get-SPTrustedIdentityTokenIssuer

$map = New-SPClaimTypeMapping "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender" -IncomingClaimTypeDisplayName "Gender" -SameAsIncoming
Add-SPClaimTypeMapping -Identity $map -TrustedIdentityTokenIssuer $ap
$ap.update();
$ap =Get-SPTrustedIdentityTokenIssuer


$map = New-SPClaimTypeMapping "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" -IncomingClaimTypeDisplayName "E-Mail" -SameAsIncoming
Add-SPClaimTypeMapping -Identity $map -TrustedIdentityTokenIssuer $ap
$ap.update();
$ap =Get-SPTrustedIdentityTokenIssuer

#$sts = Get-SPSecurityTokenServiceConfig
#$sts.LogonTokenCacheExpirationWindow = (New-TimeSpan –minutes 1)
#$sts.Update()

iisreset

#Remove-SPClaimTypeMapping -Identity $map -TrustedIdentityTokenIssuer $ap
#$ap.ClaimTypes