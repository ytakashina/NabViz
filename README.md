# NabViz
A visualization tool for Numenta Anomaly Benchmark (NAB).

![Screen shot](./image.png)

You can choose which detector to draw and change each theshold with GUI.<br>
Also you can zoom in a partcular area using mouse wheel.

## Requirements
- .NET 4.5
- Json.NET

## Downloads
The Windows binary is in the following page.
https://y-takashina.github.io/

## Build
Just open the solution file in your Visual Studio, then build.

## How to use
This application assumes the following directory structure.

```
├─bin
├─data
├─labels
└─results
```

Since NabViz is a tool only for visualization, you must download the raw data, the anomaly labels, and the calculated results from [the official NAB page](https://github.com/numenta/NAB), and put them in the same manner as the original NAB repository.

Put your binary directly under the `bin` directory.
(Don't create a `Debug` or `Release` folder)



