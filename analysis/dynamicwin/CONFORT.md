# Options de confort

Ouvrir les paramètres par l’icône DynamicWin de la zone de notification, puis **Confort**.

- Mode jeu : masque l’overlay devant Fortnite ou une application plein écran ; sa boucle graphique est mise en pause et le calcul FFT du visualiseur est suspendu. La capture audio Windows reste ouverte pour permettre la reprise. Discord continue de fonctionner.
- Exclusions : noms exacts des processus, avec ou sans `.exe`, séparés par `;` (exemple : `obs64.exe; jeu.exe`). Seule l’application au premier plan déclenche le masquage. Jusqu’à 50 noms ; pas de chemins ni de jokers. La liste normalisée est affichée après Enregistrer.
- Économie : jusqu’à 60 images/s en activité ; 15 images/s après 10 secondes sans entrée utilisateur, si aucune musique n’est en lecture. Le rythme ne dépasse pas celui de l’écran.
- Apparence : largeur, décalages horizontal et vertical, opacité de 30 à 100 %, fond en `#RRGGBB`, choix de l’écran. Les curseurs sont appliqués immédiatement et sauvegardés à la fermeture des paramètres ou avec Enregistrer en bas de page. Le champ couleur possède son bouton Appliquer.
- Raccourcis : Ctrl+Alt+D masque/affiche l’îlot ; Ctrl+Alt+P lecture/pause ; Ctrl+Alt+flèche droite/gauche morceau suivant/précédent. Désactivables ensemble. Le mode jeu et les exclusions restent prioritaires sur l’affichage manuel. Le masquage manuel est réinitialisé au lancement.
- Diagnostic : événements locaux dans `%APPDATA%/DynamicWinV2-Optimized/diagnostics`, ouverture par le bouton dédié. Deux fichiers d’environ 256 Ko maximum chacun. Démarrage, arrêt, pauses/reprises et mémoire périodique ; exceptions gérées par .NET avec noms de méthodes, sans message d’exception ni chemin source. Aucun titre musical, identifiant Discord ou envoi automatique. Les crashs natifs restent à analyser avec les rapports Windows.

Validation : tests automatisés des exclusions, couleurs, économie et rotation des journaux ; interface vérifiée visuellement ; raccourci masquer/afficher vérifié en fonctionnement. Une session de jeu prolongée n’a pas été simulée.
