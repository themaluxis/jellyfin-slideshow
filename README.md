# Jellyfin Plugin Spotlight

Plugin Jellyfin qui affiche des trailers dynamiques de votre médiathèque sur la page d'accueil, en utilisant le système [file-transformation](https://github.com/IAmParadox27/jellyfin-plugin-file-transformation).

## Fonctionnalités

- 🎬 Lecture automatique de trailers YouTube ou locaux
- 🎲 Sélection aléatoire de films/séries depuis votre bibliothèque
- 📱 Responsive (désactivé automatiquement sur mobile)
- 🎨 Overlay avec métadonnées détaillées
- ⚙️ Configuration via l'interface Jellyfin
- 🔄 Installation non-destructive (pas de modification manuelle des fichiers)

## Prérequis

1. **Jellyfin 10.10.5+**
2. **File Transformation Plugin** - Installer depuis le repo:
   ```
   https://www.iamparadox.dev/jellyfin/plugins/manifest.json
   ```

## Installation

### Méthode 1: Build depuis les sources

```bash
# Cloner le repo
git clone https://github.com/votre-username/jellyfin-plugin-spotlight.git
cd jellyfin-plugin-spotlight

# Build avec Python
python build.py

# Copier le ZIP généré vers Jellyfin
cp artifacts/Jellyfin.Plugin.Spotlight.zip /path/to/jellyfin/plugins/
```

### Méthode 2: Installation manuelle

1. Télécharger le dernier release
2. Extraire dans `/var/lib/jellyfin/plugins/Spotlight/`
3. Redémarrer Jellyfin

## Structure du projet

```
Jellyfin.Plugin.Spotlight/
├── Plugin.cs                    # Point d'entrée principal
├── Transformer.cs               # Logique d'injection HTML
├── PluginConfiguration.cs       # Configuration du plugin
├── Jellyfin.Plugin.Spotlight.csproj
├── Configuration/
│   ├── spotlight.html          # Interface Spotlight (à créer)
│   ├── spotlight.css           # Styles (à créer)
│   └── configPage.html         # Page de configuration (à créer)
├── build.py                     # Script de build
└── README.md
```

## Fichiers à ajouter

Vous devez créer le dossier `Configuration/` avec les fichiers suivants récupérés depuis [jellyfin-script-spotlight](https://github.com/JSethCreates/jellyfin-script-spotlight):

### 1. `Configuration/spotlight.html`

Copiez le fichier `spotlight.html` du repo original en adaptant:
- Remplacer les références aux variables par la config du plugin
- Ajuster les chemins des ressources

### 2. `Configuration/spotlight.css`

Copiez le fichier CSS du repo original.

### 3. `Configuration/configPage.html`

Page de configuration pour l'interface Jellyfin:

```html
<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="utf-8">
    <title>Spotlight Configuration</title>
</head>
<body>
    <div data-role="page" class="page type-interior pluginConfigurationPage">
        <div data-role="content">
            <form class="spotlightConfigForm">
                <div class="selectContainer">
                    <label for="moviesSeriesBoth">Type de média:</label>
                    <select id="moviesSeriesBoth" name="moviesSeriesBoth">
                        <option value="1">Films uniquement</option>
                        <option value="2">Séries uniquement</option>
                        <option value="3">Les deux</option>
                    </select>
                </div>

                <div class="inputContainer">
                    <label for="shuffleInterval">Intervalle slide statique (ms):</label>
                    <input type="number" id="shuffleInterval" name="shuffleInterval" min="1000" max="60000" step="1000" />
                </div>

                <div class="checkboxContainer">
                    <label>
                        <input type="checkbox" id="useTrailers" name="useTrailers" />
                        <span>Activer les trailers</span>
                    </label>
                </div>

                <div class="inputContainer">
                    <label for="defaultVolume">Volume par défaut (%):</label>
                    <input type="number" id="defaultVolume" name="defaultVolume" min="0" max="100" step="5" />
                </div>

                <button type="submit" data-theme="b">Enregistrer</button>
            </form>
        </div>
    </div>

    <script type="text/javascript">
        $('.spotlightConfigForm').on('submit', function(e) {
            e.preventDefault();
            ApiClient.getPluginConfiguration('a8f3c7e2-1d4b-4a9e-8f2c-3d7b5e6a9c1f').then(function(config) {
                config.MoviesSeriesBoth = parseInt($('#moviesSeriesBoth').val());
                config.ShuffleInterval = parseInt($('#shuffleInterval').val());
                config.UseTrailers = $('#useTrailers').is(':checked');
                config.DefaultVolume = parseInt($('#defaultVolume').val());
                
                ApiClient.updatePluginConfiguration('a8f3c7e2-1d4b-4a9e-8f2c-3d7b5e6a9c1f', config).then(function() {
                    Dashboard.alert('Configuration sauvegardée');
                });
            });
        });

        $('.spotlightConfigForm').on('pageshow', function() {
            ApiClient.getPluginConfiguration('a8f3c7e2-1d4b-4a9e-8f2c-3d7b5e6a9c1f').then(function(config) {
                $('#moviesSeriesBoth').val(config.MoviesSeriesBoth);
                $('#shuffleInterval').val(config.ShuffleInterval);
                $('#useTrailers').prop('checked', config.UseTrailers);
                $('#defaultVolume').val(config.DefaultVolume);
            });
        });
    </script>
</body>
</html>
```

## Configuration

Après installation, accédez à **Dashboard → Plugins → Spotlight** pour configurer:

- **Type de média**: Films, séries ou les deux
- **Intervalle**: Durée des slides statiques (sans trailer)
- **Trailers**: Activer/désactiver la lecture des trailers
- **Volume**: Volume par défaut (0-100%)

## Utilisation de list.txt

Pour contrôler manuellement les médias affichés:

1. Créer un fichier `list.txt` dans le dossier de configuration
2. Ajouter un ID Jellyfin par ligne
3. Le plugin utilisera uniquement ces médias

Exemple:
```
Happy Fathers Day!
4a05a2baa566acb6ea1de8edb75a56d6 The Matrix
8c4a181803702a61cc48072bd5113fb6 Breaking Bad
496528765c9937932301a1590752a7f4 Inception
```

## Fonctionnement technique

### Architecture

1. **File Transformation**: Le plugin s'enregistre auprès de File Transformation au démarrage
2. **Pattern Matching**: Détection du fichier `home-html.*.chunk.js`
3. **Injection**: Ajout de l'iframe Spotlight après `movie,series,book">`
4. **Serving**: Les fichiers HTML/CSS sont servis comme ressources embarquées

### Flux d'exécution

```
Démarrage Jellyfin
    ↓
Plugin.Constructor()
    ↓
RegisterFileTransformation()
    ↓
[Requête vers home-html.chunk.js]
    ↓
Transformer.Transform()
    ↓
Injection HTML iframe
    ↓
Chargement spotlight.html
    ↓
Affichage trailers
```

## Développement

### Prérequis

- .NET 8.0 SDK
- Python 3.8+ (pour le script de build)

### Build

```bash
# Clean + Build + Package
python build.py

# Ou manuellement
dotnet clean
dotnet build -c Release
```

### Debugging

Activer les logs Jellyfin:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "Jellyfin.Plugin.Spotlight": "Debug"
      }
    }
  }
}
```

## Troubleshooting

### Le plugin ne s'affiche pas

1. Vérifier que File Transformation est installé et actif
2. Vider le cache navigateur (Ctrl+Shift+R)
3. Consulter les logs: `/var/log/jellyfin/`

### Trailers ne se lancent pas

- Vérifier que les métadonnées contiennent des liens trailers
- Certaines vidéos YouTube refusent l'embed externe
- Consulter la console navigateur (F12)

### Erreur "pattern not found"

Le fichier chunk.js de Jellyfin a changé. Ouvrir une issue avec la version de Jellyfin.

## Compatibilité

| Version Plugin | Jellyfin |
|---------------|----------|
| 1.0.0         | 10.10.5+ |

## Licence

MIT License - Basé sur [jellyfin-script-spotlight](https://github.com/JSethCreates/jellyfin-script-spotlight) de JSethCreates

## Crédits

- **File Transformation**: [IAmParadox27](https://github.com/IAmParadox27/jellyfin-plugin-file-transformation)
- **Spotlight Original**: [JSethCreates](https://github.com/JSethCreates/jellyfin-script-spotlight)
- **Concept**: JPVenson ([PR #9095](https://github.com/jellyfin/jellyfin/pull/9095))

## Contribuer

Les contributions sont les bienvenues ! N'hésitez pas à:

1. Fork le projet
2. Créer une branche feature
3. Commit vos changements
4. Push vers la branche
5. Ouvrir une Pull Request
