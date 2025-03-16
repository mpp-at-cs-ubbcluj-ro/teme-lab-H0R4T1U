package project.moto.Repository;

import project.moto.Domain.Player;


import java.sql.Connection;
import java.sql.PreparedStatement;
import java.util.*;


public class PlayerDBRepository extends AbstractDatabaseRepository<Integer,Player>
{

    public PlayerDBRepository(Properties props) {
        super(props);
        Load();
        logger.info("Initialized PlayerDBIRepository with properties: {}", props);
    }

    /**
     * Find the entity with the given id, if it exists in memory in returns instantly, else queries ithe DBs
     * @param aInt -the id of the entity to be returned
     * id must not be null
     * @return Player - null if it doesnt exist
     */
    @Override
    public Optional<Player> findOne(Integer aInt) {
        logger.info("Finding Player by ID: " + aInt);
        if(data.containsKey(aInt))
        {
            logger.traceExit("Player MEMORY_HIT");
            return Optional.of(data.get(aInt));
        }
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("select * from Player where id = ?")) {
            preStmt.setInt(1, aInt);
            try (var result = preStmt.executeQuery()) {
                if (result.next()) {
                    int id = result.getInt("Id");
                    String name = result.getString("Name");
                    String code = result.getString("Code");
                    Integer team = result.getInt("Team");
                    Player player = new Player(name, code, team);
                    player.setId(id);
                    logger.traceExit();
                    data.put(id, player);
                    return Optional.of(player);
                }
            }
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
        logger.traceExit();
        return Optional.empty();
    }

    @Override
    public Map<Integer,Player> findAll() {
        logger.info("Finding all Players");
        return super.findAll();
    }

    /**
     * Load all Players from the database
     */
    @Override
    protected void Load(){
        Connection con = dbUtils.getConnection();
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
    }

    /**
     * Save the entity to the database and locally
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
                data.put(entity.getId(), entity);
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
     * Delete the entity from the database and locally
     * @param aInt
     * id must be not null
     * @return the deleted entity if the entity was deleted, null otherwise
     */
    @Override
    public Optional<Player> delete(Integer aInt) {
        logger.info("Deleting Player: {}", aInt);
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("delete from Player where id = ?")) {
            preStmt.setInt(1, aInt);
            preStmt.executeUpdate();
            logger.traceExit();
            return Optional.of(data.remove(aInt));
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
    }

    /**
     * Update the entity in the database and locally
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
            logger.traceExit();
            return Optional.ofNullable(data.put(entity.getId(), entity));
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }

    }
}
