using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Core.Services;

namespace WeldAdminPro.Web.Services.Quality
{
    public class PqrApplicationService
    {
        private readonly PqrRepository _repository;
        private readonly QualificationRangeEngine _rangeEngine = new();
        public PqrApplicationService(PqrRepository repository)
        {
            _repository = repository;
        }

        public List<Pqr> GetAll()
        {
            return _repository.GetAll();
        }

        public Pqr? GetById(Guid id)
        {
            return _repository.GetById(id);
        }

        public Pqr? GetByNumber(string number)
        {
            return _repository.GetByNumber(number);
        }

        public void Save(Pqr pqr)
        {
            if (pqr.Id == Guid.Empty || _repository.GetById(pqr.Id) == null)
                _repository.Add(pqr);
            else
                _repository.Update(pqr);
        }

        public void Delete(Guid id)
        {
            _repository.Delete(id);
        }

        public void CalculateQualification(Pqr pqr)
        {
            // Preserve values already calculated by the OCR parser.
            // Only calculate missing qualification data.

            if (pqr.ThicknessQualifiedMax <= 0)
            {
                var result = _rangeEngine.Calculate(pqr);

                pqr.ThicknessQualifiedMin = result.MinThickness;
                pqr.ThicknessQualifiedMax = result.MaxThickness;

                pqr.DiameterMin = result.MinDiameter;
                pqr.DiameterMax = result.MaxDiameter;
            }

            if (string.IsNullOrWhiteSpace(pqr.QualifiedPosition))
            {
                var result = _rangeEngine.Calculate(pqr);

                pqr.QualifiedPosition = result.QualifiedPosition;
            }

            if (string.IsNullOrWhiteSpace(pqr.QualifiedPNumberRange))
            {
                CalculateMaterialQualification(pqr);
            }
        }

        public void CalculateMaterialQualification(Pqr pqr)
        {
            if (string.IsNullOrWhiteSpace(pqr.PNumber))
            {
                pqr.QualifiedPNumberRange = "Not defined";
                return;
            }

            switch (pqr.PNumber.Trim())
            {
                case "1":
                    pqr.QualifiedPNumberRange = "P-No 1 only";
                    break;

                case "8":
                    pqr.QualifiedPNumberRange = "P-No 8 only (Stainless)";
                    break;

                default:
                    pqr.QualifiedPNumberRange = $"P-No {pqr.PNumber}";
                    break;
            }
        }
    }
}