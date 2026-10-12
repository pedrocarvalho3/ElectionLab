using ElectionLab.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectionLab.Infrastructure.Persistence.Configurations;

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("candidates");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ElectionId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(x => x.Number)
            .IsRequired();
        
        builder.HasIndex(x => new { x.ElectionId, x.Number })
            .IsUnique();
        
        builder.HasOne<Election>()
            .WithMany()
            .HasForeignKey(x => x.ElectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}