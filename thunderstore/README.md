# DSP Transport Stat

Press Ctrl+F to open/close transport stations window.

Great thanks to authors of LSTM and Unity Explorer.

## Features

- Listing all transport stations in every corner of your galaxy. Showing the location, state and storage of stations.
- Filtering though station type, location, name and items.
- Sorting by station location and name, ascending or descending.

# Known Issues

- Causing null pointer exception when used with LSTM.

# Todo List

- Add small bar graph to each item slot (just like those in station panel)

# Change log

## 0.0.4 -> 0.0.5

- Add chinese translations

## 0.0.3 -> 0.0.4

- Querying and sorting
- Column head

## 0.0.1 -> 0.0.3
- Change shortcut key from Ctrl+T to Ctrl+F to avoid conflict with LSTM's shortcut key.

# Some personal notes

不要把物品槽位写死成 item01 02 03 04 05 ，改成使用动态数组，减少代码冗余

在 TransportStationsWindow 中显示当前列表中一共有多少个物流站点
