import os
import openai
import time

# Read the API key from file
with open("API.txt", "r", encoding="utf-8") as file:
    APIKey = file.read().strip()

# Initialize the OpenAI client with your API key
client = openai.OpenAI(api_key=APIKey)

# Folder paths
folder_all_prompts = r'STRING_PFAD'
folder_Path_all_GPT_answers = r'STRING_PFAD'

# Set the number of runs for each prompt
numRunsForEachPrompt = 10

# Delay between each request (in seconds)
delay = 1  # You can adjust this based on your requirements

# Temperature values
# temperatures = [0.2, 0.4, 0.6, 0.8]
temperatures = [0.2]
Model_ = "gpt-4-1106-preview"

counter = 0
counter_file = 0


for filename in os.listdir(folder_all_prompts):
    if filename.endswith('.txt'):
        file_path = os.path.join(folder_all_prompts, filename)

        with open(file_path, 'r', encoding="utf-8", errors='replace') as file:
            Full_Text_Prompt = file.read()
        counter += 1

        for temperature in temperatures:
            for run in range(numRunsForEachPrompt):
                success = False
                attempts = 0
                while not success and attempts < 3:  # Retry up to 3 times
                    try:
                        # Create a chat completion with specified temperature
                        chat_completion = client.chat.completions.create(
                            messages = [{"role": "user", "content": Full_Text_Prompt}],
                            model = Model_,
                            temperature = temperature
                        )
                        GPT_Antwort = chat_completion.choices[0].message.content
                        success = True
                    except Exception as e:
                        print(f"An error occurred: {e}")
                        attempts += 1

                if success:
                    counter_file += 1
                    GPT_Name = "GPT4Turbo"
                    new_filename = filename.replace(".txt", f"_{GPT_Name}_Temp_{temperature}_Response_{run + 1}.txt")
                    new_file_path = os.path.join(folder_Path_all_GPT_answers, new_filename)
                    with open(new_file_path, 'w', encoding="utf-8") as file:
                        file.write(GPT_Antwort)

                time.sleep(delay)  # Delay between each API call
                print(f"File number {counter} -- Temperature {temperature} -- Test number {run + 1} -- Total: {counter_file}")