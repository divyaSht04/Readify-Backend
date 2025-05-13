using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend;

[Table("PendingRegistrations")]
public class PendingRegistration
{
    [Key]
    public Guid Id { get; set; }
    
    [Column("email"), MaxLength(100), Required]
    public string Email { get; set; }
    
    [Column("full_name"), MaxLength(100), Required]
    public string FullName { get; set; }
    
    [Column("phone_number"), MaxLength(100), Required]
    public string PhoneNumber { get; set; }
    
    [Column("password"), Required, MaxLength(255)]
    public string HashedPassword { get; set; }
    
    [Column("address"), MaxLength(100), Required]
    public string Address { get; set; }
    
    [Column("image"), MaxLength(100)]
    public string? ImagePath { get; set; }
    
    [Column("otp_code"), MaxLength(10), Required]
    public string OtpCode { get; set; }
    
    [Column("otp_expiry"), Required]
    public DateTime OtpExpiryTime { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 