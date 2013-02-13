all: bin\samples.sforce.com.dll

bin\samples.sforce.com.dll: simple.asmx.cs sso.asmx.cs
	csc /t:library /out:bin\samples.sforce.com.dll simple.asmx.cs sso.asmx.cs /debug+

	