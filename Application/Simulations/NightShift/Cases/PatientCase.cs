namespace Application.Simulations.NightShift.Cases;

public class PatientCase
{
    public required string Key { get; init; }

    public required string Name { get; init; }

    public required string Situation { get; init; }

    public required string Emotion { get; init; }

    public required string LearningGoal { get; init; }

    public short TensionStart { get; init; }

    public required string Opener { get; init; }

    public required PatientRecord Record { get; init; }
}

public class PatientRecord
{
    public required string Born { get; init; }

    public required string Gender { get; init; }

    public required string Admission { get; init; }

    public required string Diagnoses { get; init; }

    public required string Allergies { get; init; }

    public required string Medication { get; init; }

    public required string CareLevel { get; init; }

    public required string Risks { get; init; }

    public required string Resuscitation { get; init; }

    public required string Relatives { get; init; }
}
