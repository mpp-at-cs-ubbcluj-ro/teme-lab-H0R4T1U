package project.moto.Repository;

import project.moto.Domain.User;

import java.util.Optional;

public interface UserRepository extends Repository<Integer, User>{
    Optional<User> findByUsername(String username);
}
