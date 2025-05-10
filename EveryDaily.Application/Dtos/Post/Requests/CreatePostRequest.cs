using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace EveryDaily.Application.Dtos.Post.Requests;

public class CreatePostRequest
{
    [MaxLength(2500,ErrorMessage = "Gönderi uzunluğu 2500 karakterden fazla olamaz.")]
    public string Content { get; set; }
    public string? ImageKey { get; set; }
    public string? Id { get; set; }
   // public string? ImageUrl { get; set; }
}