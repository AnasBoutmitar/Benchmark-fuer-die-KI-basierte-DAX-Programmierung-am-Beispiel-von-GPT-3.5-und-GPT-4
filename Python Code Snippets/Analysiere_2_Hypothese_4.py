# Python script to calculate the Chi-square value and p-value for the given data
import pandas as pd
from scipy.stats import chi2_contingency

def perform_chi_square_test(data_path):
    # Laden der Daten
    data = pd.read_excel(data_path)

    # Daten für 'Calculated Tables' filtern
    calculated_tables_data = data[data['Prompt_Category'] == 'CT']
    ct_failure_success_counts = calculated_tables_data['EvaluationStatus_1'].value_counts()

    # Daten für andere Kategorien
    overall_failure_success_counts = data['EvaluationStatus_1'].value_counts()
    other_categories_counts = overall_failure_success_counts - ct_failure_success_counts

    # Erstellung einer Kontingenztafel
    contingency_table = pd.DataFrame([ct_failure_success_counts, other_categories_counts],
                                     index=['Calculated Tables', 'Other Categories'])

    # Durchführung des Chi-Quadrat-Tests
    chi2, p_value, dof, expected = chi2_contingency(contingency_table)

    return chi2, p_value, dof, expected

# Pfad zur Daten-Datei (ersetzen Sie dies durch den tatsächlichen Pfad Ihrer Datei)
data_path = r'c:\Users\³\Desktop\Bachelorarbeit\Data.xlsx'

# Test ausführen und Ergebnisse ausgeben
chi2, p_value, dof, expected = perform_chi_square_test(data_path)
print(f"Chi-square Value: {chi2}")
print(f"p-value: {p_value}")
print(f"Degrees of Freedom: {dof}")
print(f"Expected Frequencies: \n{expected}")