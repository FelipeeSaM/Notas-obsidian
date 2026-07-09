---
tags:
  - computação
  - Linux
  - Linux-script
---
## Painel superior do plasma aparece quando mostra a área de trabalho

```
#!/usr/bin/env bash  
  
PANEL_ID=87  
LAST_STATE=""  
  
apply_state() {  
   local state="$1"  
  
   if [[ "$state" == "true" ]]; then  
       busctl --user call \  
         org.kde.plasmashell \  
         /PlasmaShell \  
         org.kde.PlasmaShell \  
         evaluateScript s \  
         "panelById(${PANEL_ID}).hiding='none'" >/dev/null  
   else  
       busctl --user call \  
         org.kde.plasmashell \  
         /PlasmaShell \  
         org.kde.PlasmaShell \  
         evaluateScript s \  
         "panelById(${PANEL_ID}).hiding='autohide'" >/dev/null  
   fi  
}  
  
while true; do  
  
   CURRENT=$(  
     busctl --user get-property \  
       org.kde.KWin \  
       /KWin \  
       org.kde.KWin \  
       showingDesktop |  
       awk '{print $2}'  
   )  
  
   if [[ "$CURRENT" != "$LAST_STATE" ]]; then  
       apply_state "$CURRENT"  
       LAST_STATE="$CURRENT"  
   fi  
  
   sleep 0.5  
done
```