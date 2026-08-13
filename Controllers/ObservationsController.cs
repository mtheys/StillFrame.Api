using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StillFrame.Api.Models;

namespace StillFrame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObservationsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ObservationsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreateObservation(
            [FromBody] ObservationRequest request)
        {
            if (request.ObjectCount < 0)
                return BadRequest("ObjectCount cannot be negative.");

            string? connectionString =
                _configuration.GetConnectionString("StillFrameDatabase");

            if (string.IsNullOrWhiteSpace(connectionString))
                return StatusCode(500, "Database connection string missing.");

            const string sql = @"
                INSERT INTO Observation
                (
                    StudyId,
                    ObservationStationId,
                    ObjectClassId,
                    ObjectCount,
                    CapturedAt,
                    AverageConfidence,
                    ProcessingMilliseconds
                )
                SELECT
                    @StudyId,
                    @ObservationStationId,
                    s.ObjectClassId,
                    @ObjectCount,
                    @CapturedAt,
                    @AverageConfidence,
                    @ProcessingMilliseconds
                FROM Study s
                WHERE s.StudyId = @StudyId;

                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            ";

            try
            {
                await using SqlConnection connection =
                    new SqlConnection(connectionString);

                await connection.OpenAsync();

                await using SqlCommand command =
                    new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@StudyId", request.StudyId);
                command.Parameters.AddWithValue(
                    "@ObservationStationId",
                    request.ObservationStationId);

                command.Parameters.AddWithValue(
                    "@ObjectCount",
                    request.ObjectCount);

                command.Parameters.AddWithValue(
                    "@CapturedAt",
                    request.CapturedAt);

                command.Parameters.AddWithValue(
                    "@AverageConfidence",
                    (object?)request.AverageConfidence ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@ProcessingMilliseconds",
                    (object?)request.ProcessingMilliseconds ?? DBNull.Value);

                object? result = await command.ExecuteScalarAsync();

                if (result == null)
                    return BadRequest("Invalid StudyId.");

                return Ok(new
                {
                    success = true,
                    observationId = Convert.ToInt64(result)
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database operation failed.",
                    error = ex.Message
                });
            }
        }
    }
}