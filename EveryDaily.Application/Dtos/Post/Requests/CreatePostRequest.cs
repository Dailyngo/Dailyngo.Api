using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace EveryDaily.Application.Dtos.Post.Requests;

public class CreatePostRequest
{
    [MaxLength(1000,ErrorMessage = "Gönderi uzunluğu 1000 karakterden fazla olamaz.")]
    [MinLength(10,ErrorMessage = "Gönderi uzunluğu 10 karakterden az olamaz.")]
    public string Content { get; set; }

    public string? Id { get; set; }
   // public string? ImageUrl { get; set; }
}