# DynamicWin V2 — version recompilée et optimisée

26 octobre 2026. Basée sur DynamicWin V2.0.3 de Florian Butz et son moteur FenUI. Cette version locale modifie le code récupéré par décompilation ; elle n'est pas une release officielle.

## Lancement

Depuis la racine du workspace :

```powershell
./dist/DynamicWinV2-Optimized/Launch-Optimized.ps1
```

Ajouter `-Settings` pour ouvrir les réglages au démarrage. Le lanceur évite de lancer une deuxième instance optimisée. La version optimisée est déjà en cours d'exécution à la fin de l'intervention.

Le lanceur utilise le runtime .NET 9 du workspace et le profil `%APPDATA%/DynamicWinV2-Optimized`, séparé du profil officiel. Ce profil est initialisé avec la mise à jour automatique désactivée afin qu'une release officielle ne remplace pas les modifications. Depuis l'ajout du démarrage Windows, l'exécutable direct utilise lui aussi ce profil et enregistre le démarrage selon le réglage General / Start on Login. Voir `INTEGRATIONS.md` pour les modifications ultérieures et leurs vérifications.

Les originaux restent dans `analysis/dynamicwin/v2.0.3`. La V1 modifiée du workspace n'a pas été changée.

## Modifications

- **FenUI/FWindowLoop** : remplacement de l'attente active de fin de frame par un timer Windows haute résolution, avec repli sur `Thread.Sleep`. Priorité du thread logique ramenée de `Highest` à `Normal`. La fréquence cible de l'écran reste celle du moteur original ; aucun plafond FPS ajouté.
- **Dispatcher** : aucune liste temporaire créée lorsque sa queue est vide.
- **Notch** : planification du verrouillage d'invalidation uniquement lors d'un changement de cible, au lieu de créer une fermeture et un appel différé à chaque image. Maintien du délai de deux ticks et vérification que la vue n'est pas détruite.
- **SpectrumProcessor / AudioVisualizer** : buffers FFT réutilisés, plages de fréquences calculées à la création, fenêtre de Hann recalculée uniquement si la taille du paquet change. Conservation d'un instantané indépendant pour l'événement public, afin de préserver le comportement attendu par les extensions.
- **SmallVisualizer** : tampon de réception fixe protégé par verrou ; traitement et lecture des états graphiques sur le thread UI. Suppression du tableau supplémentaire à chaque paquet et initialisation des états avant de démarrer la capture. Valeurs non finies ramenées à zéro.
- **ActivityManager** : comparaison des types avec `typeof(T)` pour éviter l'accumulation d'activités secondaires et permettre leur désinscription.
- **Outillage** : projets .NET 9 compilables, script de build, tests, lanceur isolé, option `--settings`, journal de diagnostic facultatif via `DYNAMICWIN_LOG`.

## Validation et mesures

La compilation Release de l'application et de FenUI réussit. Les sources décompilées conservent des avertissements de nullabilité et de code généré ; elles n'ont pas été entièrement réécrites.

### Traitement audio

180 comparaisons à trois fréquences d'échantillonnage (44,1 / 48 / 96 kHz), quatre tailles de paquets et cinq fréquences de signal, sur trois passes de lissage. Les résultats stéréo flottants correspondent à l'algorithme original avec une tolérance de `1e-5`. Vérifications supplémentaires : silence, paquet vide, une seule trame et propriété des instantanés.

Allocation mesurée, hors initialisation, avec le buffer FFT original également réutilisé dans la référence :

| Calcul d'un paquet audio | Avant | Après |
|---|---:|---:|
| Octets alloués | 216 | 48 |

Soit **77,8 % de réduction des allocations de cette étape**, sans compter le tableau supplémentaire supprimé dans le widget. Cela ne signifie pas 78 % de RAM totale en moins.

Le chemin audio conserve l'hypothèse de flottants 32 bits ; les autres encodages WASAPI et les changements de périphérique ne sont pas validés. Le regroupement des canaux utilise désormais le nombre de canaux déclaré, mais la validation numérique couvre la stéréo.

### Attente de fin de frame

Microbenchmark de 500 attentes de 2 ms : environ 984 ms de temps CPU pour l'attente active contre un résultat inférieur à la granularité de mesure CPU du processus pour le timer lors du dernier test. Durée réelle : environ 1 001 ms contre 1 198 ms. Le timer économise le CPU mais peut se réveiller légèrement après l'échéance ; la fluidité sous forte charge reste à évaluer. Ne pas extrapoler ce test isolé en pourcentage de gain global.

### Application complète

Comparaison séquentielle du **même exécutable applicatif optimisé** avec FenUI officiel, puis FenUI optimisé ; même profil, réglages fermés, 12 secondes de stabilisation puis trois fenêtres de 5 secondes. Cela isole principalement les modifications du moteur, pas l'ensemble des changements applicatifs.

| FenUI | CPU total sur environ 15 s | Working set observé |
|---|---:|---:|
| Original | 578 ms | 252–253 Mio |
| Optimisé | 563 ms | 239–240 Mio |

Les mesures CPU sont proches et varient fortement entre échantillons : **aucun gain global CPU fiable n'est établi**. Les différences de mémoire sur ces exécutions courtes ne constituent pas non plus une garantie. Les premières mesures de l'exécutable officiel ne sont pas retenues pour une comparaison directe, car l'état initial de l'interface n'était pas contrôlé.

Les fichiers `measurement-original-engine.json` et `measurement-optimized-engine.json` conservent les échantillons. `verification.log` contient la compilation et les tests. La fenêtre de réglages et la navigation General → Notch ont été vérifiées visuellement. Deux avertissements de libération répétée de FText ont été observés pendant cette navigation ; aucun crash observé durant la validation. Le comportement de tous les widgets, le calendrier connecté, le Bluetooth et une session prolongée n'ont pas été testés.

## Reproduire

```powershell
# Fermer d'abord la version optimisée, puis compiler, tester et assembler :
./build-dynamicwin-v2.ps1 -Test

# Mesurer une variante (ne pas exécuter plusieurs variantes simultanément) :
./analysis/dynamicwin/Measure-Variant.ps1 -Executable ./dist/DynamicWinV2-Optimized/DynamicWinV2.exe -Label optimized-engine -KeepRunning
```

Le build utilise les dépendances originales extraites, conserve leur manifeste de résolution complet et remplace les assemblies applicatives recompilées. Les sources modifiées sont dans `optimized-v2` et `optimized-fenui`. Les sources décompilées brutes sont conservées à côté.

Attribution : https://github.com/FlorianButz/DynamicWin — Florian Butz, licence affichée CC BY-SA 4.0. Les dépendances conservent leurs propres conditions. Voir également `ANALYSE.md` pour la provenance et le hash de l'archive officielle.
