package project.moto.Repository.DatasbaseRepository;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import project.moto.Domain.Player;
import project.moto.Repository.PlayerRepository;
import project.moto.Utils.JdbcUtils;


import java.sql.Connection;
import java.sql.PreparedStatement;
import java.util.*;


public class PlayerDBRepository implements PlayerRepository
{
    private final JdbcUtils dbUtils;
    private static final Logger logger = LogManager.getLogger();
    public PlayerDBRepository(Properties props) {
        dbUtils = new JdbcUtils(props);
        logger.info("Initialized PlayerDBRepository with properties: {}", props);
    }

    /**
     * Find the entity with the given id
     * @param pId -the id of the entity to be returned
     * pId must not be null
     * @return Player - null if it doesnt exist
     */
    @Override
    public Optional<Player> findOne(Integer pId) {
        logger.info("Finding Player by ID: " + pId);
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("select * from Player where id = ?")) {
            preStmt.setInt(1, pId);
            try (var result = preStmt.executeQuery()) {
                if (result.next()) {
                    int id = result.getInt("Id");
                    String name = result.getString("Name");
                    String code = result.getString("Code");
                    Integer team = result.getInt("Team");
                    Player player = new Player(name, code, team);
                    player.setId(id);
                    logger.traceExit("Player found");
                    return Optional.of(player);
                }
            }
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
        logger.traceExit("Player not found");
        return Optional.empty();
    }

    /**
     * Find all entities in the database
     * @return Map<Integer,Player> - all entities
     */
    @Override
    public Map<Integer,Player> findAll() {
        logger.info("Finding all Players");
        Connection con = dbUtils.getConnection();
        Map<Integer,Player> data = new HashMap<>();
        try (var preStmt = con.prepareStatement("select * from Player")) {
            try (var result = preStmt.executeQuery()) {
                while (result.next()) {
                    int id = result.getInt("Id");
                    String name = result.getString("Name");
                    String code = result.getString("Code");
                    Integer team = result.getInt("Team");
                    Player player = new Player(name, code, team);
                    player.setId(id);
                    data.put(id,player);
                }

            }
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }

        logger.traceExit("Found {} Players in database", data.size());
        return data;
    }

    /**
     * Save the entity to the database
     * @param entity
     * entity must be not null
     * @return entity if the entity was saved, null otherwise
     */
    @Override
    public Optional<Player> save(Player entity) {
        logger.info("Saving Player: {}", entity.getId());
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement(
                "insert into Player (Name, Code, Team) values (?, ?, ?)",
                PreparedStatement.RETURN_GENERATED_KEYS)) {
            preStmt.setString(1, entity.getName());
            preStmt.setString(2, entity.getCode());
            preStmt.setInt(3, entity.getTeam());
            var result = preStmt.executeUpdate();
            if (result > 0) {
                try (var generatedKeys = preStmt.getGeneratedKeys()) {
                    if (generatedKeys.next()) {
                        entity.setId(generatedKeys.getInt(1));
                    }
                }
                logger.traceExit("Player saved with id" + entity.getId());
                return Optional.of(entity);
            }
            logger.traceExit();
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
        logger.traceExit("Player was not saved");
        return Optional.empty();
    }

    /**
     * Delete the entity from the database
     * @param pID
     * id must be not null
     * @return the deleted entity if the entity was deleted, null otherwise
     */
    @Override
    public Optional<Player> delete(Integer pID) {
        logger.info("Deleting Player: {}", pID);
        Connection con = dbUtils.getConnection();
        Optional<Player> player = findOne(pID);
        if(player.isPresent()) {
            try (var preStmt = con.prepareStatement("delete from Player where id = ?")) {
                preStmt.setInt(1, pID);
                preStmt.executeUpdate();
                logger.traceExit("Player {} deleted", pID);
                return player;
            } catch (Exception e) {
                logger.error(e.getMessage());
                throw new RuntimeException(e);
            }
        }
        logger.traceExit("Player was not deleted");
        return Optional.empty();
    }

    /**
     * Update the entity in the database
     * @param entity - the entity to update
     * @return old entity if the entity was updated, null otherwise
     */
    @Override
    public Optional<Player> update(Player entity) {
        logger.info("Updating Player: {}", entity.getId());
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("update Player set Name = ?, Code = ?, Team = ? where id = ?")) {
            preStmt.setString(1, entity.getName());
            preStmt.setString(2, entity.getCode());
            preStmt.setInt(3, entity.getTeam());
            preStmt.setInt(4, entity.getId());
            preStmt.executeUpdate();
            logger.traceExit("Player updated succesfully");
            return Optional.of(entity);
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }

    }
}
