;#DropDB CollectionsResolutionEasyTest

#Application CollectionsResolution.Win
#Application CollectionsResolution.Web
#IfDef CollectionsResolution.Win

*Action About...
 
*Action OK

#EndIf

#IfDef CollectionsResolution.Web
!Action Navigation(My Details)
#EndIf
