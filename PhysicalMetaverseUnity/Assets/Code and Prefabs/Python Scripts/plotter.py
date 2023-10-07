import matplotlib.pyplot as plt

# Your data
data = [
    91, 91, 88, 83, 80, 75, 69, 64, 61, 56, 53, 48, 45, 42, 40, 40, 37, 34, 32, 32, 
    29, 29, 26, 26, 24, 24, 24, 21, 21, 21, 21, 18, 18, 18, 18, 16, 16, 16, 16, 16, 
    16, 16, 16, 16
]

# Create a plot
plt.plot(data)

# Add labels and title
plt.xlabel('Real distance')
plt.ylabel('Measured distance')
#range of y axis 0 255
plt.ylim(0, 100)
plt.title('Data Plot')



# Your data points
data = [
    0.008695652173913044, 0.00909090909090909, 0.009615384615384616, 0.010101010101010102,
    0.010416666666666666, 0.010416666666666666, 0.010752688172043012, 0.01098901098901099,
    0.011764705882352941, 0.012048192771084338, 0.012987012987012988, 0.014492753623188406,
    0.015625, 0.01694915254237288, 0.018867924528301886, 0.020833333333333332,
    0.023809523809523808, 0.025, 0.02702702702702703, 0.03125, 0.03125, 0.034482758620689655,
    0.038461538461538464, 0.041666666666666664, 0.041666666666666664, 0.047619047619047616,
    0.047619047619047616, 0.047619047619047616, 0.05555555555555555, 0.05555555555555555,
    0.05555555555555555, 0.0625, 0.0625, 0.0625, 0.0625, 0.0625
]

#multiply data
data = [x * 700 for x in data]
# Create a list of indices to use as x-values
x = list(range(len(data)))

# Create the plot
#plt.plot(x, data, marker='o', linestyle='-')
plt.plot(data)

# Label the axes
plt.xlabel('Real distance')
plt.ylabel('Measured distance')

# Title of the plot
plt.title('Linearized')

# Show the plot
plt.show()