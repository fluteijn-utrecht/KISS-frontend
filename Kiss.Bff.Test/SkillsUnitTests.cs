using Microsoft.AspNetCore.Mvc;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.NieuwsEnWerkinstructies.Features;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class SkillsControllerUnitTests : TestHelper
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeDatabase();
        }

        [TestMethod]
        public async Task GetSkills_ReturnsOkResult_WithSkills()
        {
            // Arrange
            var dbContext = new BeheerDbContext(_dbContextOptions);
            dbContext.Skills.AddRange(new List<Skill>
            {
                new Skill { Id = 1, Naam = "Skill 1" },
                new Skill { Id = 2, Naam = "Skill 2" }
            });
            dbContext.SaveChanges();

            var controller = new SkillsController(dbContext);

            // Act
            var actionResult = controller.GetSkills();
            var okResult = actionResult?.Result as OkObjectResult;
            var resultSkills = okResult?.Value as IAsyncEnumerable<SkillViewModel>;

            // Assert
            var skillsList = new List<SkillViewModel>();
            var enumerator = resultSkills?.GetAsyncEnumerator();
            if (enumerator != null)
            while (await enumerator.MoveNextAsync())
            {
                skillsList.Add(enumerator.Current);
            }

            Assert.AreEqual(2, skillsList.Count);
            Assert.AreEqual(1, skillsList[0].Id);
            Assert.AreEqual("Skill 1", skillsList[0].Naam);
            Assert.AreEqual(2, skillsList[1].Id);
            Assert.AreEqual("Skill 2", skillsList[1].Naam);
        }

        [TestMethod]
        public async Task GetSkill_ReturnsOkResult_WithValidId()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new SkillsController(dbContext);
            dbContext.Skills.Add(new Skill { Id = 1, Naam = "Skill 1" });
            dbContext.SaveChanges();

            // Act
            var actionResult = await controller.GetSkill(1, CancellationToken.None);
            var resultSkill = actionResult.Value as SkillViewModel;

            // Assert
            Assert.AreEqual(1, resultSkill?.Id);
            Assert.AreEqual("Skill 1", resultSkill?.Naam);
        }

        [TestMethod]
        public async Task GetSkill_ReturnsNotFoundResult_WithInvalidId()
        {
            // Act
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new SkillsController(dbContext);
            var actionResult = await controller.GetSkill(1, CancellationToken.None);
            var notFoundResult = actionResult.Result as NotFoundResult;

            // Assert
            Assert.IsNotNull(notFoundResult);
        }

        [TestMethod]
        public async Task PutSkill_ReturnsNoContentResult_WithValidId()
        {
            // Arrange
            var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new SkillsController(dbContext);
            dbContext.Skills.Add(new Skill { Id = 1, Naam = "Skill 1" });
            dbContext.SaveChanges();
            var updatedSkill = new SkillPutModel { Naam = "Updated Skill" };

            // Act
            var actionResult = await controller.PutSkill(1, updatedSkill, CancellationToken.None);
            var noContentResult = actionResult as NoContentResult;

            // Assert
            Assert.IsNotNull(noContentResult);
            var skill = dbContext.Skills.FirstOrDefault(s => s.Id == 1);
            Assert.IsNotNull(skill);
            Assert.AreEqual("Updated Skill", skill.Naam);
        }

        [TestMethod]
        public async Task PutSkill_ReturnsNotFoundResult_WithInvalidId()
        {
            // Arrange
            var updatedSkill = new SkillPutModel { Naam = "Updated Skill" };
            var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new SkillsController(dbContext);

            // Act
            var actionResult = await controller.PutSkill(1, updatedSkill, CancellationToken.None);
            var notFoundResult = actionResult as NotFoundResult;

            // Assert
            Assert.IsNotNull(notFoundResult);
        }

        [TestMethod]
        public async Task PostSkill_ReturnsCreatedAtActionResult_WithValidModel()
        {
            // Arrange
            var dbContext = new BeheerDbContext(_dbContextOptions);
            var newSkill = new SkillPostModel { Naam = "New Skill" };
            var controller = new SkillsController(dbContext);

            // Act
            var actionResult = await controller.PostSkill(newSkill, CancellationToken.None);
            var createdAtActionResult = actionResult.Result as CreatedAtActionResult;
            var resultSkill = createdAtActionResult?.Value as Skill;

            // Assert
            Assert.IsNotNull(createdAtActionResult);
            Assert.IsNotNull(resultSkill);
            Assert.AreEqual("New Skill", resultSkill.Naam);
            var skill = dbContext.Skills.FirstOrDefault(s => s.Id == resultSkill.Id);
            Assert.IsNotNull(skill);
            Assert.AreEqual("New Skill", skill.Naam);
        }

        [TestMethod]
        public async Task DeleteSkill_ReturnsNoContentResult_WithValidId()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            dbContext.Skills.Add(new Skill { Id = 1, Naam = "Skill 1" });
            dbContext.SaveChanges();
            var controller = new SkillsController(dbContext);
            // Act
            var actionResult = await  controller.DeleteSkill(1, CancellationToken.None);
            var noContentResult = actionResult as NoContentResult;

            // Assert
            Assert.IsNotNull(noContentResult);
            var skill = dbContext.Skills.FirstOrDefault(s => s.Id == 1);
            Assert.IsNotNull(skill);
            Assert.IsTrue(skill.IsDeleted);

        }

        [TestMethod]
        public async Task DeleteSkill_ReturnsNotFoundResult_WithInvalidId()
        {
            // Act
            var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new SkillsController(dbContext);
            var actionResult = await controller.DeleteSkill(1, CancellationToken.None);
            var notFoundResult = actionResult as NotFoundResult;

            // Assert
            Assert.IsNotNull(notFoundResult);
        }
    }
}
