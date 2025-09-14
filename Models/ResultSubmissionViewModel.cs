using System.ComponentModel.DataAnnotations;
using VcBlazor.Data.Entities;

namespace VcBlazor.Models
{
    public class ResultSubmissionViewModel
    {
        [Display(Name = "Bureau de vote")]
        [Required(ErrorMessage = "Le bureau de vote est obligatoire")]
        public int PollingStationId { get; set; }

        [Display(Name = "Type de soumission")]
        public string SubmissionType { get; set; } = "final";

        [Display(Name = "Électeurs inscrits")]
        [Range(0, int.MaxValue, ErrorMessage = "Le nombre d'électeurs inscrits doit être positif")]
        public int RegisteredVoters { get; set; }

        [Display(Name = "Total des votes")]
        [Range(0, int.MaxValue, ErrorMessage = "Le total des votes doit être positif")]
        public int TotalVotes { get; set; }

        [Display(Name = "Votes blancs")]
        [Range(0, int.MaxValue, ErrorMessage = "Les votes blancs doivent être positifs")]
        public int BlankVotes { get; set; } = 0;

        [Display(Name = "Votes nuls")]
        [Range(0, int.MaxValue, ErrorMessage = "Les votes nuls doivent être positifs")]
        public int InvalidVotes { get; set; } = 0;

        [Display(Name = "Statut")]
        public string Status { get; set; } = "pending";

        [Display(Name = "Observations")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Display(Name = "Soumis par")]
        public int? SubmittedBy { get; set; }

        // Résultats détaillés par candidat
        public List<CandidateResultViewModel> CandidateResults { get; set; } = new();

        // Propriétés calculées
        public int ValidVotes => CandidateResults.Sum(c => c.Votes);
        public double TurnoutRate => RegisteredVoters > 0 ? (double)TotalVotes / RegisteredVoters * 100 : 0;

        // Validation personnalisée
        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();

            if (TotalVotes > RegisteredVoters)
            {
                errors.Add("Le total des votes ne peut pas dépasser les électeurs inscrits");
            }

            if (ValidVotes + BlankVotes + InvalidVotes != TotalVotes)
            {
                errors.Add("La somme des votes valides, blancs et nuls doit égaler le total des votes");
            }

            if (CandidateResults.Any(c => c.Votes < 0))
            {
                errors.Add("Les votes par candidat ne peuvent pas être négatifs");
            }

            return errors.Count == 0;
        }
    }

    public class CandidateResultViewModel
    {
        public int CandidateId { get; set; }
        
        [Display(Name = "Candidat")]
        public string CandidateName { get; set; } = string.Empty;
        
        [Display(Name = "Parti")]
        public string Party { get; set; } = string.Empty;

        [Display(Name = "Nombre de votes")]
        [Range(0, int.MaxValue, ErrorMessage = "Le nombre de votes doit être positif")]
        public int Votes { get; set; } = 0;

        [Display(Name = "Pourcentage")]
        public double Percentage { get; set; } = 0;
    }
}