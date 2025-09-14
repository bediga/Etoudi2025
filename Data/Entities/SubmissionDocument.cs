using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("submission_documents")]
    public class SubmissionDocument
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("submission_id")]
        public int SubmissionId { get; set; }

        [Required]
        [Column("document_type")]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        [Column("original_file_name")]
        public string? OriginalFileName { get; set; }

        [Required]
        [Column("file_path")]
        public string FilePath { get; set; } = string.Empty;

        [Column("file_size")]
        public long FileSize { get; set; }

        [Column("upload_date")]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        [Column("checksum")]
        public string? Checksum { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("mime_type")]
        public string? MimeType { get; set; }

        [Column("is_image")]
        public bool IsImage { get; set; } = false;

        [Column("uploaded_by")]
        public int? UploadedBy { get; set; }

        [Column("status")]
        public string Status { get; set; } = "active";

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(SubmissionId))]
        public virtual ResultSubmission? Submission { get; set; }

        [ForeignKey(nameof(UploadedBy))]
        public virtual User? UploadedByUser { get; set; }
    }
}