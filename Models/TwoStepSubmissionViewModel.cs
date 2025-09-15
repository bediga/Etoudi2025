using System.ComponentModel.DataAnnotations;

namespace VcBlazor.Models
{
    public class TwoStepSubmissionViewModel
    {
        // Étape courante (1 ou 2)
        public int CurrentStep { get; set; } = 1;

        // Informations du bureau de vote (Étape 1)
        [Required(ErrorMessage = "Le bureau de vote est requis")]
        public int? PollingStationId { get; set; }

        public string? PollingStationName { get; set; }
        public string? Region { get; set; }
        public string? Department { get; set; }
        public string? Arrondissement { get; set; }
        public string? Commune { get; set; }

        // Totaux (Étape 1)
        [Required(ErrorMessage = "Le nombre total de votes est requis")]
        [Range(0, int.MaxValue, ErrorMessage = "Le nombre de votes ne peut pas être négatif")]
        public int TotalVotes { get; set; }

        [Required(ErrorMessage = "Le nombre de votes blancs est requis")]
        [Range(0, int.MaxValue, ErrorMessage = "Le nombre de votes blancs ne peut pas être négatif")]
        public int BlankVotes { get; set; }

        [Required(ErrorMessage = "Le nombre de votes nuls est requis")]
        [Range(0, int.MaxValue, ErrorMessage = "Le nombre de votes nuls ne peut pas être négatif")]
        public int NullVotes { get; set; }

        // Note: Ces propriétés ne sont pas dans ResultSubmission, on les ajoutera dans les notes ou on créera des champs séparés

        [Required(ErrorMessage = "Le type de soumission est requis")]
        public string SubmissionType { get; set; } = "final";

        public int RegisteredVoters { get; set; }

        // Candidats et votes (Étape 2)
        public List<CandidateVoteModel> CandidateVotes { get; set; } = new List<CandidateVoteModel>();

        // ID de la soumission si elle existe déjà
        public int? ResultSubmissionId { get; set; }

        // Propriétés calculées
        public decimal TurnoutRate => RegisteredVoters > 0 ? (decimal)TotalVotes / RegisteredVoters * 100 : 0;

        public int TotalCandidateVotes => CandidateVotes.Sum(c => c.Votes);

        public bool IsStep1Valid => PollingStationId.HasValue && TotalVotes >= 0 && BlankVotes >= 0 && NullVotes >= 0;

        public bool IsStep2Valid => CandidateVotes.Any() && TotalCandidateVotes + BlankVotes + NullVotes == TotalVotes;
    }

    public class CandidateVoteModel
    {
        public int CandidateId { get; set; }
        public string? CandidateName { get; set; }
        public string? Party { get; set; }

        [Required(ErrorMessage = "Le nombre de votes est requis")]
        [Range(0, int.MaxValue, ErrorMessage = "Le nombre de votes ne peut pas être négatif")]
        public int Votes { get; set; }

        public decimal Percentage { get; set; }
    }

    public class PollingStationSimpleModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Region { get; set; }
        public string? Department { get; set; }
        public string? Arrondissement { get; set; }
        public string? Commune { get; set; }
        public int RegisteredVoters { get; set; }
    }

    public class CandidateSimpleModel
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Party { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}