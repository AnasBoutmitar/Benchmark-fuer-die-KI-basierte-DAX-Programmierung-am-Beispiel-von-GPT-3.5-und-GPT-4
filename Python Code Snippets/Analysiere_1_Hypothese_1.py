# Python script to calculate the Chi-square value and p-value for the given data
import pandas as pd
from scipy.stats import chi2_contingency

def calculate_chi_square(data_path):
    # Load the data
    data = pd.read_excel(data_path)

    # Extracting relevant data
    language_success_data = data[['Prompt_Language', 'EvaluationStatus_1']]

    # Creating a contingency table
    language_success_counts = language_success_data.value_counts().unstack()

    # Performing the Chi-square test
    chi2, p_value, dof, expected = chi2_contingency(language_success_counts)

    return chi2, p_value, dof, expected

# Path to the data file (replace with the actual path of the data file)
data_path = r'c:\Users\³\Desktop\Bachelorarbeit\Data.xlsx'

# Calling the function and printing the results
chi2, p_value, dof, expected = calculate_chi_square(data_path)

print(f"Chi-square Value: {chi2}")
print(f"p-value: {p_value}")
print(f"Degrees of Freedom: {dof}")
print(f"Expected Frequencies: \n{expected}")

