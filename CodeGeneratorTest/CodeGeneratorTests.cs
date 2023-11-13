using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScavengeRUs.Data;
using ScavengeRUs.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Moq;

namespace GenerateUniqueCodeTest
{
    [TestClass]
    public class CodeGeneratorTests
    {
        private readonly CodeGenerator _codeGenerator;
        private readonly ApplicationDbContext _context;
        private readonly Mock<ApplicationDbContext> _mockContext;

        public CodeGeneratorTests()
        {
            _mockContext = new Mock<ApplicationDbContext>();


            // Create options for InMemory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Create instance of DbContext
            _context = new ApplicationDbContext(options);

            // Pass the DbContext instance to the CodeGenerator
            _codeGenerator = new CodeGenerator(_context);
        }

        [TestMethod]
        public void GenerateUniqueCode_ShouldReturnUniqueCode()
        {
            var generatedCodes = new HashSet<string>();
            for (int i = 0; i < 500; i++) // Adjusted number of iterations
            {
                var code = _codeGenerator.GenerateUniqueCode();
                Assert.IsTrue(Regex.IsMatch(code, @"^[A-Za-z]+[A-Za-z]+[0-9]{3,4}$")); // regex pattern
                Assert.IsFalse(generatedCodes.Contains(code), $"Duplicate code generated: {code}");
                generatedCodes.Add(code);
            }
        }



        // Additional tests for exception handling, etc. will be needed


    }
}
