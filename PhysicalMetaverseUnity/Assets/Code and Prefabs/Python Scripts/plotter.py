import matplotlib.pyplot as plt
import numpy as np

# Facts.
xmin = 0.3  # Minimum distance of the sensor
xmax = 5  # Maximum distance of the sensor
ymax_sensor = 115  # At 0.3m
ymin_sensor = 16  # At 5m

# Sensor measurements.
y1 = np.array([
    115, 110, 104,  99,  96,  96,  93,  91,  85,  83,  77,
    69,  64,  59,  53,  48,  42,  40,  37,  32,  32,  29,
    26,  24,  24,  21,  21,  21,  18,  18,  18,  16,  16,
    16,  16,  16
])  # Measured values.
# y1 = np.array([
#     91, 91, 88, 83, 80, 75, 69, 64, 61, 56, 53, 48, 45, 42, 40, 40, 37, 34, 32, 32, 
#     29, 29, 26, 26, 24, 24, 24, 21, 21, 21, 21, 18, 18, 18, 18, 16, 16, 16, 16, 16, 
#     16, 16, 16, 16
# ])
y1 = np.clip(y1, ymin_sensor, ymax_sensor)  # Clip the values to the sensor range.
y1 = (y1 - ymin_sensor) / (ymax_sensor - ymin_sensor)  # Normalize the values.
y1 = y1 * (xmax - xmin) + xmin  # Scale the values to the real distance range.


# Create the x axis.
x1 = np.linspace(xmin, xmax, len(y1))


# Model the sensor measurements.
y2_model = np.poly1d(np.polyfit(x1, y1, 2))
y2 = y2_model(x1)


# Digital Week (DW) linearization.
y3 = np.array([
    0.008695652173913044, 0.00909090909090909, 0.009615384615384616, 0.010101010101010102,
    0.010416666666666666, 0.010416666666666666, 0.010752688172043012, 0.01098901098901099,
    0.011764705882352941, 0.012048192771084338, 0.012987012987012988, 0.014492753623188406,
    0.015625, 0.01694915254237288, 0.018867924528301886, 0.020833333333333332,
    0.023809523809523808, 0.025, 0.02702702702702703, 0.03125, 0.03125, 0.034482758620689655,
    0.038461538461538464, 0.041666666666666664, 0.041666666666666664, 0.047619047619047616,
    0.047619047619047616, 0.047619047619047616, 0.05555555555555555, 0.05555555555555555,
    0.05555555555555555, 0.0625, 0.0625, 0.0625, 0.0625, 0.0625
])  # Measured values.
y3 = (y3 - np.min(y3)) / (np.max(y3) - np.min(y3))  # Normalize the values.
y3 = y3 * (xmax - xmin) + xmin  # Scale the values to the real distance range.


# Linearize the data.
f = lambda x: 12 / (0.5 * x**0.5 + 1) - 5.2  # Function to linearize the data
y4 = f(y1)  # Linearize the data


# Linearize the model.
y5 = f(y2)  # Linearize the model


# Plot the data.
plt.plot(x1, y1, label="Sensor")
plt.plot(x1, y2, label="Sensor model", linestyle="-.")
plt.plot(x1, y3, label="DW lin.")
plt.plot(x1, y4, label="Post DW lin.", linestyle="-.")
plt.plot(x1, y5, label="Post DW lin. (model)", linestyle="-.")
plt.plot([xmin, xmax], [xmin, xmax], label="y = x", linestyle="--")
plt.legend()
plt.grid()
plt.xlabel("Real distance [m]")
plt.ylabel("Measured distance [m]")
plt.title("Distance linearization")
plt.show()
