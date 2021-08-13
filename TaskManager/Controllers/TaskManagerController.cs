using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Utility;
using Process = TaskManager.Models.Process;

namespace TaskManager.Controllers
{
    [Produces("application/json")]
    // In case, a newer version of our API is released in the future
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TaskManagerController : ControllerBase
    {
        private readonly TaskManagerContext _context;
        private readonly ILogger _log;
        //private readonly int _capacity;
        private readonly int _capacity = Convert.ToInt32(WebAppSettings.GetTaskManagerCapacity());

        public TaskManagerController(TaskManagerContext context, ILogger<TaskManagerController> logger)
        {
            _context = context;
            _log = logger;
        }

        /// <summary>
        /// Retrieves all processes
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        /// GET /api/v1/TaskManager/GetProcesses
        ///
        /// </remarks>
        /// <returns>Returns all the processes created until now</returns>
        /// <response code="201">If the processes are found and returned succesfully</response>
        /// <response code="500">If there is any error</response>   
        // GET: api/v1/TaskManager/GetProcesses
        [HttpGet("GetProcesses")]
        //[Route("GetProcesses")]
        public async Task<ActionResult<IEnumerable<ProcessDTO>>> GetProcesses()
        {
            List<ProcessDTO> result = await _context.Processes.OrderBy(p => p.CreatedAt).ThenBy(p => p.Priority).ThenBy(p => p.PID).AsNoTracking()
                .Select(x => ProcessToDTOForGetAll(x))
                .ToListAsync();
            return result;
        }

        /// <summary>
        /// Returns just a single process matching the id
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        /// GET /api/v1/TaskManager/GetProcess/{id}
        /// </remarks>
        /// <param name="id"></param>
        /// <returns>A process</returns>
        /// <response code="201">If the process is found and returned succesfully</response>
        /// <response code="500">If there is any error</response>   
        // GET: api/v1/TaskManager/GetProcess/5
        [HttpGet("GetProcess/{id}")]
        //[Route("GetProcess")]
        public async Task<ActionResult<ProcessDTO>> GetProcess(long id)
        {
            Stopwatch sw = new Stopwatch();
            //string error = null;
            sw.Start();

            try
            {
                Process process = await _context.Processes.FindAsync(id);

                if (process == null)
                {
                    return NotFound();
                }

                return ProcessToDTO(process);
            }
            catch (Exception ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(500, new MessageError(ex.Message, ex.StackTrace));
            }
            finally
            {
                sw.Stop();
                _log.TaskManagerSingleLogError($"Task Manager Controller. Elapsed : { sw.Elapsed.TotalMilliseconds}");
            }
        }



        /// <summary>
        /// Creates a new process
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/TaskManager/CreateNewProcessDefault
        ///{
        /// "PID": 5,
        ///"Priority": "high"
        ///}
        /// </remarks>
        /// <param name="processDTO"></param>
        /// <returns>A newly created Process</returns>
        /// <response code="201">Returns the newly created process</response>
        /// <response code="400">If the process is null</response>   
        // POST: api/v1/TaskManager/CreateNewProcessDefault
        [HttpPost("CreateNewProcessDefault")]
        //[Route("CreateNewProcessDefault")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status507InsufficientStorage)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProcessDTO>> CreateNewProcessDefault(ProcessDTO processDTO)
        {
            try
            {
                if (processDTO == null)
                {
                    return BadRequest();
                }
                if (_context.Processes.Count() == _capacity)
                {
                    return StatusCode(507, new MessageError("New process creation cannot be accepted since the processes in Task Manager has exceeded the capacity! Please kill existing processes before creating a new one", ""));
                }
                else
                {
                    Process process = ProcessDTOToProcess(processDTO);
                    _context.Add(process);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetProcess), new { id = process.PID }, ProcessToDTO(process));
                }
            }
            catch (ArgumentNullException ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(404, new MessageError(ex.Message, ex.StackTrace));
            }
            catch (Exception ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(500, new MessageError(ex.Message, ex.StackTrace));
            }
            finally
            {
            }

        }

        /// <summary>
        /// Creates a new process
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/TaskManager/CreateNewProcessFIFO
        ///{
        /// "PID": 5,
        ///"Priority": "high"
        ///}        
        /// </remarks>
        /// <param name="processDTO"></param>
        /// <returns>A newly created Process</returns>
        /// <response code="201">Returns the newly created process</response>
        /// <response code="400">If the process is null</response>   
        // POST: api/v1/TaskManager/CreateNewProcessFIFO
        [HttpPost("CreateNewProcessFIFO")]
        //[Route("CreateNewProcessFIFO")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProcessDTO>> CreateNewProcessFIFO(ProcessDTO processDTO)
        {
            try
            {
                if (processDTO == null)
                {
                    return BadRequest();
                }

                if (_context.Processes.Count() == _capacity)
                {
                    List<Process> processes = new List<Process>();
                    processes = _context.Processes.ToList();
                    await KillProcessDefault(processes[0].PID);
                }
                Process process = ProcessDTOToProcess(processDTO);
                _context.Add(process);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProcess), new { id = process.PID }, ProcessToDTO(process));

            }
            catch (ArgumentNullException ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(404, new MessageError(ex.Message, ex.StackTrace));
            }
            catch (Exception ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(500, new MessageError(ex.Message, ex.StackTrace));
            }
            finally
            {
            }

        }

        /// <summary>
        /// Creates a new process
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/TaskManager/CreateNewProcessPriorityBased
        ///{
        /// "PID": 5,
        ///"Priority": "high"
        ///}        
        /// </remarks>
        /// <param name="processDTO"></param>
        /// <returns>A newly created Message</returns>
        /// <response code="201">Returns the newly created process</response>
        /// <response code="400">If the process is null</response>   
        // POST: api/v1/TaskManager/CreateNewProcessPriorityBased
        [HttpPost("CreateNewProcessPriorityBased")]
        //[Route("CreateNewProcessPriorityBased")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProcessDTO>> CreateNewProcessPriorityBased(ProcessDTO processDTO)
        {
            try
            {
                if (processDTO == null)
                {
                    return BadRequest();
                }

                //Validate the model first
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Process requestProcess = ProcessDTOToProcess(processDTO);
                bool isDeletedLowerPriorityProcess = false;

                if (_context.Processes.Count() == _capacity)
                {
                    List<Process> processes = new List<Process>();
                    processes = _context.Processes.OrderBy(p => p.CreatedAt).ThenBy(p => p.Priority).Where(p => p.Priority < requestProcess.Priority).ToList();
                    if (processes != null && processes.Count() > 0)
                    {
                        await KillProcessDefault(processes[0].PID);
                        isDeletedLowerPriorityProcess = true;
                    }
                    else
                    {
                        return StatusCode(400, new MessageError("Either there is no process with priority less than the requested one or there is some error", ""));
                    }
                }
                if (isDeletedLowerPriorityProcess || _context.Processes.Count() < _capacity)
                {
                    _context.Add(requestProcess);
                    await _context.SaveChangesAsync();
                    return CreatedAtAction(nameof(GetProcess), new { id = requestProcess.PID }, ProcessToDTO(requestProcess));
                }

                return StatusCode(500, new MessageError("Something unexpected error has occured!", ""));
            }
            catch (ArgumentNullException ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(404, new MessageError(ex.Message, ex.StackTrace));
            }
            catch (Exception ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(500, new MessageError(ex.Message, ex.StackTrace));
            }
            finally
            {
            }

        }


        /// <summary>
        /// Kills a process
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        /// DELETE /api/v1/TaskManager/KillProcessDefault/1
        /// </remarks>
        /// <param name="id"></param>
        /// <returns>Kills a Process with specified id</returns>
        /// <response code="200">Returns the deleted process whose id matches the parameter</response>
        /// <response code="400">If the process is null</response>   
        // DELETE: api/v1/TaskManager/KillProcessDefault/5
        [HttpDelete("KillProcessDefault/{id}")]
        //[Route("KillProcessDefault")]
        public async Task<IActionResult> KillProcessDefault(long id)
        {
            try
            {
                Process process = await _context.Processes.FindAsync(id);
                if (process == null)
                {
                    return NotFound();
                }

                _context.Processes.Remove(process);
                await _context.SaveChangesAsync();

                return Content("Process with id " + process.PID + " is deleted");
            }
            catch (ArgumentNullException ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(404, new MessageError(ex.Message, ex.StackTrace));
            }
            catch (Exception ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(500, new MessageError(ex.Message, ex.StackTrace));
            }
            finally
            {
            }
        }

        /// <summary>
        /// Kills a process
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        /// DELETE /api/v1/TaskManager/KillAllProcessesOfPriority/low
        /// </remarks>
        /// <param name="id"></param>
        /// <returns>Kills the processes with specified priority</returns>
        /// <response code="200">Returns the deleted process whose id matches the parameter</response>
        /// <response code="400">If the process is null</response>   
        // DELETE: api/v1/TaskManager/KillAllProcessesOfPriority/{priority}
        [HttpDelete("KillAllProcessesOfPriority/{priority}")]
        //[Route("KillAllProcessesOfPriority")]
        public async Task<IActionResult> KillAllProcessesOfPriority(string priority)
        {
            try
            {

                List<Process> processList = new List<Process>();
                StringBuilder pIDListString = new StringBuilder();

                processList = await _context.Processes.AsNoTracking().Where(p => Enum.GetName(typeof(Priority), p.Priority).ToLower().Equals(priority.ToLower())).ToListAsync();
                if (processList == null || processList.Count == 0)
                {
                    return NotFound();
                }
                foreach (Process prc in processList)
                {
                    _context.Processes.Remove(prc);

                    if (processList.Count == 1)
                    {
                        pIDListString.Append(prc.PID.ToString());
                    }
                    else
                    {
                        pIDListString.Append(prc.PID.ToString() + ", ");
                    }
                }
                await _context.SaveChangesAsync();

                return Content("Processes of priority " + priority + " with id " + pIDListString + " are deleted");
            }
            catch (ArgumentNullException ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(404, new MessageError(ex.Message, ex.StackTrace));
            }
            catch (Exception ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(500, new MessageError(ex.Message, ex.StackTrace));
            }
            finally
            {
            }
        }

        /// <summary>
        /// Kills a process
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        /// DELETE /api/v1/TaskManager/KillAllProcesses
        /// </remarks>
        /// <returns>Success Message is deleted the processes successfully</returns>
        /// <response code="200">Deletes all the running processes</response>
        /// <response code="400">If the process is null</response>   
        // DELETE: api/v1/TaskManager/KillAllProcesses
        [HttpDelete("KillAllProcesses")]
        //[Route("KillAllProcesses")]
        public async Task<IActionResult> KillAllProcesses()
        {
            try
            {
                List<Process> processList = await _context.Processes.AsNoTracking().ToListAsync();
                if (processList == null || processList.Count == 0)
                {
                    return NotFound();
                }

                foreach (Process prc in processList)
                {
                    _context.Processes.Remove(prc);
                }
                await _context.SaveChangesAsync();

                return Content("All the running processes are deleted");
            }
            catch (ArgumentNullException ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(404, new MessageError(ex.Message, ex.StackTrace));
            }
            catch (Exception ex)
            {
                _log.TaskManagerSingleLogError(ex.Message + " Stack trace: " + ex.StackTrace);
                return StatusCode(500, new MessageError(ex.Message, ex.StackTrace));
            }
            finally
            {
            }

        }

        #region ProcessController Helper code

        /// <summary>
        /// Method used by controller to convert process to data transfer object
        /// </summary>
        /// <param name="process"></param>
        /// <returns>ProcessDTO object</returns>
        private static ProcessDTO ProcessToDTO(Process process)
        {
            return new ProcessDTO
            {
                Priority = Enum.GetName(typeof(Priority), process.Priority),
                PID = process.PID,
                CreatedAt = process.CreatedAt.ToString("dd-mm-YYYY HH:mm")
            };
        }

        /// <summary>
        /// Method used by controller to convert process to data transfer object
        /// </summary>
        /// <param name="process"></param>
        /// <returns>ProcessDTO object</returns>
        private static ProcessDTO ProcessToDTOForGetAll(Process process)
        {
            return new ProcessDTO
            {
                PID = process.PID,
                Priority = Enum.GetName(typeof(Priority), process.Priority),
                CreatedAt = process.CreatedAt.ToString("dd-mm-YYYY HH:mm")
            };
        }



        /// <summary>
        /// Method used by controller to convert process DTO to process
        /// </summary>
        /// <param name="processDTO"></param>
        /// <returns>Process object</returns>
        private static Process ProcessDTOToProcess(ProcessDTO processDTO)
        {
            return new Process
            {
                PID = processDTO.PID,
                Priority = (Priority)Enum.Parse(typeof(Priority), processDTO.Priority),
                CreatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Method to check if process exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true</returns> if it exists
        /// <returns>true</returns> if it does not exists
        private bool ProcessExists(long id)
        {
            return _context.Processes.Any(e => e.PID == id);
        }
        #endregion

    }
}

