using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using TaskManager.Controllers;
using TaskManager.Models;
using Xunit;

namespace TaskManager.UnitTests
{
    public class TaskManagerControllerUnitTests
    {
        private readonly Mock<ILogger<TaskManagerController>> _mockLogger = new Mock<ILogger<TaskManagerController>>();

        #region Get By Id
        [Fact]
        public async void Task_GetProcessById_Return_OkResult()
        {
            // Arrange
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_GetProcessById_Return_OkResult));

            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);
            int id = 1;

            // Act
            ActionResult<ProcessDTO> response = await controller.GetProcess(id);
            dbContext.Dispose();

            // Assert
            Assert.IsType<ProcessDTO>(response.Value);
        }

        [Fact]
        public async void Task_GetProcessById_Return_NotFoundResult()
        {
            // Arrange
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_GetProcessById_Return_NotFoundResult));
            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);
            int id = 100;

            // Act
            ActionResult<ProcessDTO> response = await controller.GetProcess(id);
            dbContext.Dispose();

            //Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async void Task_GetProcessById_Return_BadRequestResult()
        {

            // Arrange
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_GetProcessById_Return_BadRequestResult));
            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);
            int id = int.Parse("-1");

            // Act
            ActionResult<ProcessDTO> response = await controller.GetProcess(id);
            dbContext.Dispose();

            //Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }


        [Fact]
        public async void Task_GetProcessById_MatchResult()
        {
            // Arrange
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_GetProcessById_MatchResult));
            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);
            int id = 1;

            // Act
            ActionResult<ProcessDTO> response = await controller.GetProcess(id);
            ProcessDTO value = response.Value;
            dbContext.Dispose();

            //Assert
            Assert.IsType<ProcessDTO>(value);


            Assert.Equal(1, value.PID);
            Assert.Equal("low", value.Priority);
        }

        #endregion

        #region Get All

        [Fact]
        public async void Task_GetProcesss_Return_OkResult()
        {
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_GetProcesss_Return_OkResult));
            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);

            //Act
            ActionResult<IEnumerable<ProcessDTO>> data = await controller.GetProcesses();

            //Assert
            Assert.IsAssignableFrom<IEnumerable<ProcessDTO>>(data.Value);
            Assert.True((data.Value as List<ProcessDTO>).Count > 0);
        }

        [Fact]
        public void Task_GetProcesss_Return_BadRequestResult()
        {
            //Arrange
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_GetProcesss_Return_BadRequestResult));
            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);

            //Act
            System.Threading.Tasks.Task<ActionResult<IEnumerable<ProcessDTO>>> data = controller.GetProcesses();
            data = null;

            if (data != null)
            {
                //Assert
                Assert.IsType<BadRequestResult>(data);
            }
        }

        [Fact]
        public async void Task_GetProcesss_MatchResult()
        {
            //Arrange
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_GetProcesss_MatchResult));
            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);

            //Act
            ActionResult<IEnumerable<ProcessDTO>> data = await controller.GetProcesses();
            List<ProcessDTO> value = (List<ProcessDTO>)data.Value;

            //Assert
            Assert.IsAssignableFrom<IEnumerable<ProcessDTO>>(data.Value);


            Assert.Equal(1, value[0].PID);
            Assert.Equal("low", value[0].Priority);

            Assert.Equal(2, value[1].PID);
            Assert.Equal("low", value[1].Priority);
        }

        #endregion

        #region Add New Process

        [Fact]
        public async void Task_Add_ValidData_Return_OkResult()
        {
            //Arrange
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_Add_ValidData_Return_OkResult));
            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);
            ProcessDTO post = new ProcessDTO
            {
                PID = 5,
                Priority = "high"
            };

            //Act
            ActionResult<ProcessDTO> data = await controller.CreateNewProcessDefault(post);

            //Assert
            Assert.IsType<CreatedAtActionResult>(data.Result);
        }

        [Fact]
        public async void Task_Add_InvalidData_Return_BadRequest()
        {
            //Arrange
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_Add_InvalidData_Return_BadRequest));
            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);
            ProcessDTO post = null;

            //Act            
            ActionResult<ProcessDTO> data = await controller.CreateNewProcessDefault(post);

            //Assert
            Assert.IsType<BadRequestResult>(data.Result);
        }

        [Fact]
        public async void Task_Add_ValidData_MatchResult()
        {
            //Arrange
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_Add_ValidData_MatchResult));
            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);
            ProcessDTO post = new ProcessDTO
            {
                PID = 6,
                Priority = "medium"
            };

            //Act
            ActionResult<ProcessDTO> data = await controller.CreateNewProcessDefault(post);

            //Assert
            Assert.IsType<CreatedAtActionResult>(data.Result);
        }

        #endregion

        #region Delete process

        [Fact]
        public async void Task_Delete_Process_Return_OkResult()
        {
            //Arrange
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_Delete_Process_Return_OkResult));
            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);
            int postId = 2;

            //Act
            IActionResult data = await controller.KillProcessDefault(postId);

            //Assert
            Assert.IsType<ContentResult>(data);
        }

        [Fact]
        public async void Task_Delete_Process_Return_NotFoundResult()
        {
            //Arrange
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_Delete_Process_Return_NotFoundResult));
            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);
            int postId = 8;

            //Act
            IActionResult data = await controller.KillProcessDefault(postId);

            //Assert
            Assert.IsType<NotFoundResult>(data);
        }

        [Fact]
        public async void Task_Delete_Return_BadRequestResult()
        {
            //Arrange
            Data.TaskManagerContext dbContext = TaskManagerContextMocker.GetProcessesContextForTests(nameof(Task_Delete_Return_BadRequestResult));
            TaskManagerController controller = new TaskManagerController(dbContext, _mockLogger.Object);
            int postId = -1;

            //Act
            IActionResult data = await controller.KillProcessDefault(postId);

            //Assert
            Assert.IsType<NotFoundResult>(data);
        }

        #endregion
    }
}
