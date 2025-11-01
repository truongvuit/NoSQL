using Core.DTOs.Common;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IFileService _fileService;

        public UploadController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("cv")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<FileUploadResponse>>> UploadCV(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponse<FileUploadResponse>
                    {
                        Success = false,
                        Message = "Vui lòng chọn file CV để upload"
                    });
                }

                using var stream = file.OpenReadStream();
                var result = await _fileService.UploadFileAsync(stream, file.FileName, file.Length, file.ContentType, "cv");

                return Ok(new ApiResponse<FileUploadResponse>
                {
                    Success = true,
                    Message = "Upload CV thành công",
                    Data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<FileUploadResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<FileUploadResponse>
                {
                    Success = false,
                    Message = "Lỗi khi upload file: " + ex.Message
                });
            }
        }

        [HttpPost("image")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<FileUploadResponse>>> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponse<FileUploadResponse>
                    {
                        Success = false,
                        Message = "Vui lòng chọn ảnh để upload"
                    });
                }

                using var stream = file.OpenReadStream();
                var result = await _fileService.UploadFileAsync(stream, file.FileName, file.Length, file.ContentType, "image");

                return Ok(new ApiResponse<FileUploadResponse>
                {
                    Success = true,
                    Message = "Upload ảnh thành công",
                    Data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<FileUploadResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<FileUploadResponse>
                {
                    Success = false,
                    Message = "Lỗi khi upload ảnh: " + ex.Message
                });
            }
        }

        [HttpDelete("{fileType}/{fileName}")]
        [Authorize(Roles = "admin,recruiter")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteFile(string fileType, string fileName)
        {
            try
            {
                var fullPath = $"{fileType}/{fileName}";
                var result = await _fileService.DeleteFileAsync(fullPath);

                if (!result)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Không tìm thấy file"
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Xóa file thành công",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Lỗi khi xóa file: " + ex.Message
                });
            }
        }

        [HttpGet("validate")]
        [Authorize]
        public ActionResult<ApiResponse<object>> ValidateFile([FromQuery] string fileType, [FromQuery] long fileSize, [FromQuery] string fileName)
        {
            try
            {
                // Validation using service
                if (!_fileService.ValidateFile(fileName, fileSize, fileType, out var errors))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Validation thất bại",
                        Errors = errors
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "File hợp lệ"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
