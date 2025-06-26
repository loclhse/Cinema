using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class CinemaRoomConfiguration : IEntityTypeConfiguration<CinemaRoom>
    {
        public void Configure(EntityTypeBuilder<CinemaRoom> builder)
        {
            builder.HasKey(c => c.Id);
            builder.ToTable("CinemaRoom");

            // Npgsql sẽ tự serialize/deserialize JsonDocument <=> jsonb
            builder.Property(c => c.LayoutJson)
                   .HasColumnType("jsonb");

            builder.Property(c => c.OriginalLayoutJson)
                   .HasColumnType("jsonb");

            builder.HasMany(c => c.Seats)
                   .WithOne(s => s.CinemaRoom)
                   .HasForeignKey(s => s.CinemaRoomId);
        }
    }
    //public class CinemaRoomConfiguration : IEntityTypeConfiguration<CinemaRoom>
    //{
    //    public void Configure(EntityTypeBuilder<CinemaRoom> builder)
    //    {
    //        builder.HasKey(c => c.Id);
    //        builder.ToTable("CinemaRoom");

    //        builder.Property(c => c.LayoutJson)
    //            .HasConversion(
    //                v => ConvertJsonObjectToString(v),
    //                v => ConvertStringToJsonObject(v)
    //            )
    //            .HasColumnType("jsonb");

    //        builder.Property(c => c.OriginalLayoutJson)
    //            .HasConversion(
    //                v => ConvertJsonObjectToString(v),
    //                v => ConvertStringToJsonObject(v)
    //            )
    //            .HasColumnType("jsonb");

    //        builder.HasMany(c => c.Seats)
    //               .WithOne(s => s.CinemaRoom)
    //               .HasForeignKey(s => s.CinemaRoomId);
    //    }

    //    private static string? ConvertJsonObjectToString(JsonObject? jsonObject)
    //    {
    //        return jsonObject?.ToJsonString();
    //    }

    //    private static JsonObject ConvertStringToJsonObject(string? jsonString)
    //    {
    //        if (string.IsNullOrWhiteSpace(jsonString))
    //        {
    //            return new JsonObject();
    //        }

    //        try
    //        {
    //            var parsedNode = JsonNode.Parse(jsonString);
    //            return parsedNode?.AsObject() ?? new JsonObject();
    //        }
    //        catch (JsonException)
    //        {
    //            // Log error if needed
    //            return new JsonObject();
    //        }
    //    }
    //}
}