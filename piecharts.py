import pandas as pd
import matplotlib.pyplot as plt

# Read the CSV file
file_path = 'Virtual Test Finali Tesi.csv'
df = pd.read_csv(file_path)

# Extract columns related to statements
statement_columns = df.columns[6:20]

# Create pie charts for each statement
for column in statement_columns:
    # Count the occurrences of each response
    counts = df[column].value_counts()

    # Plot a pie chart
    plt.figure()
    plt.pie(counts, labels=counts.index, autopct='%1.1f%%', startangle=90)
    plt.title(f'Pie Chart for: {column}')
    plt.show()
