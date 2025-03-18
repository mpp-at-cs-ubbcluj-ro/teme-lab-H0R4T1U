package project.moto.Repository.DatasbaseRepository;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import project.moto.Domain.User;
import project.moto.Repository.UserRepository;
import project.moto.Utils.JdbcUtils;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.util.HashMap;
import java.util.Map;
import java.util.Optional;
import java.util.Properties;

public class UserDBRepository implements UserRepository {

    private final JdbcUtils dbUtils;
    private static final Logger logger = LogManager.getLogger();
    public UserDBRepository(Properties props) {
        this.dbUtils = new JdbcUtils(props);
        logger.info("Initialized TeamDBRepository with properties: {}", props);

    }

    /**
     * Find the entity with the given id
     * @param pId -the id of the entity to be returned
     * pId must not be null
     * @return User - null if it doesnt exist
     */
    @Override
    public Optional<User> findOne(Integer pId) {
        logger.info("Finding User by ID: " + pId);
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("select * from User where id = ?")) {
            preStmt.setInt(1, pId);
            try (var result = preStmt.executeQuery()) {
                if (result.next()) {
                    int id = result.getInt("Id");
                    String username = result.getString("Username");
                    String password = result.getString("Password");
                    User user = new User(username,password);
                    user.setId(id);
                    logger.traceExit("user found");
                    return Optional.of(user);
                }
            }
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
        logger.traceExit("User not found");
        return Optional.empty();
    }

    /**
     * Find all entities in the database
     * @return Map<Integer,User> - all entities
     */
    @Override
    public Map<Integer,User> findAll() {
        logger.info("Finding all Users");
        Connection con = dbUtils.getConnection();
        Map<Integer,User> data = new HashMap<>();
        try (var preStmt = con.prepareStatement("select * from User")) {
            try (var result = preStmt.executeQuery()) {
                while (result.next()) {
                    int id = result.getInt("Id");
                    String username = result.getString("Username");
                    String password = result.getString("Password");
                    User user = new User(username,password);
                    user.setId(id);
                    data.put(id,user);
                }

            }
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }

        logger.traceExit("Found {} Users in database", data.size());
        return data;
    }

    /**
     * Save the entity to the database
     * @param entity
     * entity must be not null
     * @return entity if the entity was saved, null otherwise
     */
    @Override
    public Optional<User> save(User entity) {
        logger.info("Saving User: {}", entity.getId());
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement(
                "insert into User (Username, Password) values (?, ?)",
                PreparedStatement.RETURN_GENERATED_KEYS)) {
            preStmt.setString(1, entity.getUsername());
            preStmt.setString(2, entity.getPassword());
            var result = preStmt.executeUpdate();
            if (result > 0) {
                try (var generatedKeys = preStmt.getGeneratedKeys()) {
                    if (generatedKeys.next()) {
                        entity.setId(generatedKeys.getInt(1));
                    }
                }
                logger.traceExit("User saved with id" + entity.getId());
                return Optional.of(entity);
            }
            logger.traceExit();
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
        logger.traceExit("User was not saved");
        return Optional.empty();
    }

    /**
     * Delete the entity from the database
     * @param pID
     * id must be not null
     * @return the deleted entity if the entity was deleted, null otherwise
     */
    @Override
    public Optional<User> delete(Integer pID) {
        logger.info("Deleting User: {}", pID);
        Connection con = dbUtils.getConnection();
        Optional<User> user = findOne(pID);
        if(user.isPresent()) {
            try (var preStmt = con.prepareStatement("delete from User where id = ?")) {
                preStmt.setInt(1, pID);
                preStmt.executeUpdate();
                logger.traceExit("User {} deleted", pID);
                return user;
            } catch (Exception e) {
                logger.error(e.getMessage());
                throw new RuntimeException(e);
            }
        }
        logger.traceExit("User was not deleted");
        return Optional.empty();
    }

    /**
     * Update the entity in the database
     * @param entity - the entity to update
     * @return old entity if the entity was updated, null otherwise
     */
    @Override
    public Optional<User> update(User entity) {
        logger.info("Updating User: {}", entity.getId());
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("update User set Username = ?, Password = ? where id = ?")) {
            preStmt.setString(1, entity.getUsername());
            preStmt.setString(2, entity.getPassword());
            preStmt.setInt(3, entity.getId());
            preStmt.executeUpdate();
            logger.traceExit("User updated succesfully");
            return Optional.of(entity);
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }

    }

    @Override
    public Optional<User> findByUsername(String username) {
        logger.info("Finding User by Username: " + username);
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("select * from User where Username = ?")) {
            preStmt.setString(1, username);
            try (var result = preStmt.executeQuery()) {
                if (result.next()) {
                    int id = result.getInt("Id");
                    String password = result.getString("Password");
                    User user = new User(username,password);
                    user.setId(id);
                    logger.traceExit("user found");
                    return Optional.of(user);
                }
            }
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
        logger.traceExit("User not found");
        return Optional.empty();
    }
}
