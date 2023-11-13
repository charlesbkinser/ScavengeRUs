# ScavengeRUs Code Generation Documentation

## Introduction
This part contains the `CodeGenerator` class (in the services folder) and its associated tests for the ScavengeRUs project. The `CodeGenerator` is designed to generate unique access codes, which are a combination of words and numbers, ensuring uniqueness through database checks.

## CodeGenerator Class

### Overview
The `CodeGenerator` class is responsible for generating unique access codes for the ScavengeRUs teams in the application. It combines two random words from a predefined list with a random number, and checks against the database to ensure the code's uniqueness.

### Features
- **GenerateUniqueCode**: Generates a unique access code by combining words and numbers.
- **Exception Handling**: Custom exception (`CodeGenerationException`) to handle errors during code generation.

## CodeGeneratorTests Class

### Overview
`CodeGeneratorTests` ensures the functionality of the `CodeGenerator` class is working as expected. It primarily focuses on testing the uniqueness and format of the generated codes.

### Key Test
- **GenerateUniqueCode_ShouldReturnUniqueCode**: Validates that each generated code is unique and conforms to the specified format.

## Getting Started
To use the `CodeGenerator` in your project:
1. Ensure you have an instance of `ApplicationDbContext`.
2. Instantiate `CodeGenerator` with the database context.
3. Call `GenerateUniqueCode` to receive a unique access code.

## Testing
Run the `CodeGeneratorTests` to ensure the functionality of the code generation is as expected. The tests use an in-memory database for isolation.

