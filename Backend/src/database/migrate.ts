import fs from 'fs';
import path from 'path';
import { Database } from './connection';
import { logger } from '../utils/logger';
import dotenv from 'dotenv';

dotenv.config();

async function migrate() {
  try {
    await Database.initialize();
    logger.info('Starting database migration...');

    const schemaPath = path.join(__dirname, 'schema.sql');
    const schema = fs.readFileSync(schemaPath, 'utf-8');

    // Split by semicolon and execute each statement
    const statements = schema
      .split(';')
      .map(s => s.trim())
      .filter(s => s.length > 0 && !s.startsWith('--'));

    for (const statement of statements) {
      try {
        await Database.query(statement);
        logger.debug(`Executed: ${statement.substring(0, 50)}...`);
      } catch (error: any) {
        // Ignore "already exists" errors
        if (error.code === '42P07' || error.code === '42710') {
          logger.warn(`Skipped (already exists): ${statement.substring(0, 50)}...`);
        } else {
          throw error;
        }
      }
    }

    logger.info('Database migration completed successfully!');
  } catch (error) {
    logger.error('Migration failed:', error);
    process.exit(1);
  } finally {
    await Database.close();
  }
}

migrate();
