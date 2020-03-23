## RForms
- From some time earlier than 2011-10-06:
```
				//if (ssTagsOpen.Count==1) //when </*> starts
				if (iTagNow!=iBaseNode) rformarr[iTagNow].sContent=RString.SafeSubstring(ParentNode.sContent,Child_ContentStart,iParsingAt-Child_ContentStart);
```
