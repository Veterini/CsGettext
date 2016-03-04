# CsGettext
Utilities for using gettext in C#

## Use in C# code:
    `Strings.T("My string to be translate");`
    `Strings.Tp("I have {Occurence} string to be translate", "I have {Occurence} strings to be translate", count);`
    `Strings.Tc("Go", "The strategy game");`
    `Strings.Tc("Go", "The verb");`
  
## Use in WPF:
    `{gtc:GetText 'My string to be translate'}`
    `{gtc:GetText 'I have \\{Occurence\\} string to be translate',Plural='I have \\{Occurence\\} strings to be translate',Occurence=12}`
    `{gtc:GetText 'Go', Context='The strategy game'}`
    `{gtc:GetText 'Go', Context='The verb'}`

## TODO-list:
Nuget
Extraction of strings from WPF
