---
tags:
  - "#computação"
  - "#Linux"
  - "#Linux-script"
nivel:
---
## Muda o refresh rate do monitor para melhorar o FPS bugado

```
#!/bin/bash  
kscreen-doctor output.DP-1.mode.7  
sleep 1  
kscreen-doctor output.DP-1.mode.2  
notify-send "GPU clock" "Modeset forçado - MCLK deve estar destravado"  
EOF
```
