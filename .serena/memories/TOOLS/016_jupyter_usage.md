# Jupyter Notebooks Usage Guide

## Purpose
Jupyter Notebooks provide an interactive computing environment for data analysis, visualization, and prototyping. Useful for analyzing Unity project data, logs, and metrics.

## Configuration
**VSCode Extension**: `Jupyter`
**Python Environment**: `.venv/` with data science packages

## Directory Structure
```
notebooks/
├── project_analysis.ipynb    # Overall project health
├── code_metrics.ipynb        # Code complexity analysis
├── network_traffic.ipynb     # Multiplayer traffic analysis
└── performance_profiling.ipynb # Unity performance data
```

## When to Use
- Analyzing Unity logs and profiler data
- Visualizing code metrics
- Exploring project statistics
- Prototyping data processing scripts
- Creating reports with visualizations

## Core Concepts

### Cells
- **Code cells**: Execute Python code
- **Markdown cells**: Documentation and explanations
- **Output cells**: Results, charts, tables

### Kernel
The Python runtime that executes code. Restart if:
- Variables need clearing
- Memory issues occur
- Code changes require fresh state

## Output Patterns

### Code Cell Output
```python
# Input
df = pd.read_csv('metrics.csv')
print(df.shape)

# Output
(1500, 12)
```

### Visualization Output
Charts rendered inline:
- Line plots
- Bar charts
- Heatmaps
- Network graphs

### Error Output
```python
---------------------------------------------------------------------------
FileNotFoundError                         Traceback (most recent call last)
<ipython-input-1> in <module>
----> 1 df = pd.read_csv('missing.csv')

FileNotFoundError: [Errno 2] No such file or directory: 'missing.csv'
```

## AI Interpretation Guide

### Understanding Notebook State
- Cells execute in order (usually)
- Variables persist between cells
- Kernel state can become stale
- Restart kernel to clear state

### Interpreting Outputs
| Output Type | Meaning |
|-------------|---------|
| DataFrame display | Tabular data preview |
| Plot/chart | Visual data representation |
| Error traceback | Code execution failed |
| Warning | Non-fatal issue |
| Print output | Debug/info messages |

### Common Data Analysis Patterns
```python
# Load Unity logs
import pandas as pd
logs = pd.read_csv('Logs/Player.log', sep='\t')

# Filter errors
errors = logs[logs['Level'] == 'Error']

# Group by type
error_counts = errors.groupby('Type').size()

# Visualize
error_counts.plot(kind='bar')
```

## Useful Packages for MLCreator

### Data Analysis
```python
import pandas as pd      # DataFrames
import numpy as np       # Numerical computing
import json              # Parse Unity JSON
```

### Visualization
```python
import matplotlib.pyplot as plt  # Basic plots
import seaborn as sns           # Statistical plots
import plotly.express as px     # Interactive plots
```

### Unity-Specific
```python
# Parse Unity YAML
import yaml

# Read Unity logs
def parse_unity_log(path):
    # Custom parsing logic
    pass

# Analyze profiler data
def analyze_profiler(json_path):
    with open(json_path) as f:
        return json.load(f)
```

## Example Notebooks

### Project Analysis
```python
# Count files by type
import os
from collections import Counter

extensions = []
for root, dirs, files in os.walk('Assets/'):
    for f in files:
        ext = os.path.splitext(f)[1]
        extensions.append(ext)

Counter(extensions).most_common(10)
```

### Code Metrics
```python
# Lines of code per file
import glob

loc = {}
for cs_file in glob.glob('Assets/**/*.cs', recursive=True):
    with open(cs_file, encoding='utf-8') as f:
        loc[cs_file] = len(f.readlines())

# Top 10 largest files
sorted(loc.items(), key=lambda x: -x[1])[:10]
```

### Network Traffic Analysis
```python
# Parse network log
import re

pattern = r'\[RPC\] (\w+) sent to (\d+) clients, (\d+) bytes'
matches = re.findall(pattern, log_content)

# Analyze RPC frequency
rpc_counts = Counter(m[0] for m in matches)
```

## VSCode Integration
- Extension: `Jupyter`
- Create: `Ctrl+Shift+P` → "Create New Jupyter Notebook"
- Run cell: `Shift+Enter`
- Run all: `Ctrl+Shift+Enter`
- Variable explorer: Sidebar panel

## Best Practices
1. **Clear outputs before commit**: Reduces git noise
2. **Document cells**: Use markdown for explanations
3. **Modular cells**: One concept per cell
4. **Version data**: Don't modify source files
5. **Use virtual environment**: Isolated dependencies

## Troubleshooting
| Issue | Solution |
|-------|----------|
| Kernel not starting | Check Python path |
| Import errors | Install missing packages |
| Memory issues | Restart kernel, load less data |
| Slow execution | Profile code, optimize |
| Charts not showing | Add `%matplotlib inline` |
