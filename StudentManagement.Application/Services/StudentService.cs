using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;

namespace StudentManagement.Application.Services;

public class StudentService
{
    private readonly IStudentRepository _repository;

    public StudentService(IStudentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Student>> GetStudentsAsync()
    {
        return await _repository.GetAllAsync();
    }
    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddStudentAsync(Student student)
    {
        await _repository.AddAsync(student);
    }

    public async Task UpdateStudentAsync(Student student)
    {
        await _repository.UpdateAsync(student);
    }

    public async Task DeleteStudentAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
}