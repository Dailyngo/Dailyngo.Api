using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using EveryDaily.Core.Entity;
using MongoDB.Bson;

namespace EveryDaily.Domain.Documents;

public class MessageDoc : DocBase
{
    public string Content { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public bool IsRead { get; set; }
}